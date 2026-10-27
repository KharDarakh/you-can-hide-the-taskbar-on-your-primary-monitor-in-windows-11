using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using YCHTTOPYMIW11.Core.Interop;
using YCHTTOPYMIW11.Core.Models;

namespace YCHTTOPYMIW11.Core.Services
{
    public sealed class WorkAreaManager : IWorkAreaManager
    {
        private readonly ILogger<WorkAreaManager> _logger;
        private readonly ConcurrentDictionary<string, RECT> _originalWorkAreas = new(StringComparer.OrdinalIgnoreCase);

        public WorkAreaManager(ILogger<WorkAreaManager> logger)
        {
            _logger = logger;
        }

        public RECT GetCurrentWorkArea(DisplayInfo display)
        {
            return DesktopSessionHelper.RunOnInteractiveDesktop(() =>
            {
                var mi = MONITORINFOEX.Create();
                if (NativeMethods.GetMonitorInfo(display.MonitorHandle, ref mi))
                {
                    return mi.rcWork;
                }

                if (display.IsPrimary)
                {
                    NativeMethods.SystemParametersInfoGet(NativeConstants.SPI_GETWORKAREA, 0, out RECT spiWork, 0);
                    return spiWork;
                }

                return display.MonitorBounds;
            });
        }

        public bool ExpandWorkAreaToFull(DisplayInfo display)
        {
            return DesktopSessionHelper.RunOnInteractiveDesktop(() =>
            {
                // Cache original work area before making any change
                var currentWork = GetCurrentWorkArea(display);
                _originalWorkAreas.TryAdd(display.DeviceName, currentWork);

                RECT fullBounds = display.MonitorBounds;
                _logger.LogInformation("Expanding work area for {Device} from {Current} to {Full}", display.DeviceName, currentWork, fullBounds);

                // SystemParametersInfoSet with SPI_SETWORKAREA accepts ANY monitor's coordinates in virtual screen space.
                // Crucial: passing fWinIni = 0 avoids broadcasting WM_SETTINGCHANGE to Shell_TrayWnd,
                // which would cause Explorer's AppBar handler to revert the work area back!
                bool success = NativeMethods.SystemParametersInfoSet(
                    NativeConstants.SPI_SETWORKAREA,
                    0,
                    ref fullBounds,
                    0);

                if (!success)
                {
                    int err = Marshal.GetLastWin32Error();
                    _logger.LogError("SystemParametersInfoSet(SPI_SETWORKAREA) failed for {Device} with error {Error}", display.DeviceName, err);
                }
                else
                {
                    _logger.LogInformation("Successfully updated work area for {Device} to {Bounds}", display.DeviceName, fullBounds);
                }

                // Immediately resize any maximized windows on this monitor to eliminate any bottom gap
                AdjustMaximizedWindowsOnDisplay(display, fullBounds);

                // Notify desktop views and open application windows so maximized bounds adapt
                RefreshDesktopLayout();

                return success;
            });
        }

        public bool RestoreWorkArea(DisplayInfo display)
        {
            return DesktopSessionHelper.RunOnInteractiveDesktop(() =>
            {
                if (!_originalWorkAreas.TryGetValue(display.DeviceName, out RECT orig))
                {
                    _logger.LogWarning("No recorded original work area for {Device}", display.DeviceName);
                    return false;
                }

                _logger.LogInformation("Restoring work area for {Device} to {Orig}", display.DeviceName, orig);

                bool success = NativeMethods.SystemParametersInfoSet(
                    NativeConstants.SPI_SETWORKAREA,
                    0,
                    ref orig,
                    0);

                if (!success)
                {
                    int err = Marshal.GetLastWin32Error();
                    _logger.LogError("Failed restoring work area for {Device}: error {Error}", display.DeviceName, err);
                }

                AdjustMaximizedWindowsOnDisplay(display, orig);
                RefreshDesktopLayout();
                _originalWorkAreas.TryRemove(display.DeviceName, out _);
                return success;
            });
        }

        private void AdjustMaximizedWindowsOnDisplay(DisplayInfo display, RECT targetArea)
        {
            try
            {
                IntPtr primaryTray = NativeMethods.FindWindow("Shell_TrayWnd", null);
                IntPtr secondaryTray = NativeMethods.FindWindow("Shell_SecondaryTrayWnd", null);

                NativeMethods.EnumWindows((hWnd, lParam) =>
                {
                    if (hWnd != primaryTray && hWnd != secondaryTray &&
                        NativeMethods.IsWindowVisible(hWnd) &&
                        NativeMethods.IsZoomed(hWnd))
                    {
                        IntPtr hMon = NativeMethods.MonitorFromWindow(hWnd, NativeConstants.MONITOR_DEFAULTTONEAREST);
                        if (hMon == display.MonitorHandle)
                        {
                            NativeMethods.SetWindowPos(
                                hWnd,
                                IntPtr.Zero,
                                targetArea.Left,
                                targetArea.Top,
                                targetArea.Width,
                                targetArea.Height,
                                NativeConstants.SWP_NOZORDER | NativeConstants.SWP_NOACTIVATE | NativeConstants.SWP_FRAMECHANGED);
                        }
                    }
                    return true;
                }, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not adjust maximized windows for {Device}", display.DeviceName);
            }
        }

        public void RestoreAllWorkAreas()
        {
            DesktopSessionHelper.RunOnInteractiveDesktop(() =>
            {
                foreach (var kvp in _originalWorkAreas.ToArray())
                {
                    RECT orig = kvp.Value;
                    _logger.LogInformation("Restoring work area for {Device} to {Orig}", kvp.Key, orig);
                    NativeMethods.SystemParametersInfoSet(NativeConstants.SPI_SETWORKAREA, 0, ref orig, 0);
                }
                _originalWorkAreas.Clear();
                RefreshDesktopLayout();
            });
        }

        public void RefreshDesktopLayout()
        {
            DesktopSessionHelper.RunOnInteractiveDesktop(() =>
            {
                IntPtr progman = NativeMethods.FindWindow("Progman", null);
                IntPtr defView = IntPtr.Zero;
                IntPtr listView = IntPtr.Zero;

                if (progman != IntPtr.Zero)
                {
                    defView = NativeMethods.FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);
                }

                if (defView == IntPtr.Zero)
                {
                    NativeMethods.EnumWindows((hWnd, lParam) =>
                    {
                        var sb = new System.Text.StringBuilder(256);
                        NativeMethods.GetClassName(hWnd, sb, 256);
                        if (sb.ToString() == "WorkerW")
                        {
                            IntPtr child = NativeMethods.FindWindowEx(hWnd, IntPtr.Zero, "SHELLDLL_DefView", null);
                            if (child != IntPtr.Zero)
                            {
                                defView = child;
                                progman = hWnd;
                            }
                        }
                        return true;
                    }, IntPtr.Zero);
                }

                if (defView != IntPtr.Zero)
                {
                    listView = NativeMethods.FindWindowEx(defView, IntPtr.Zero, "SysListView32", null);
                }

                // 1. Send targeted WM_SETTINGCHANGE to Progman, DefView, ListView
                UIntPtr dummy;
                if (progman != IntPtr.Zero)
                {
                    NativeMethods.SendMessageTimeout(progman, NativeConstants.WM_SETTINGCHANGE, (IntPtr)NativeConstants.SPI_SETWORKAREA, "WorkArea", 0x0002 /* SMTO_ABORTIFHUNG */, 200, out dummy);
                }
                if (defView != IntPtr.Zero)
                {
                    NativeMethods.SendMessageTimeout(defView, NativeConstants.WM_SETTINGCHANGE, (IntPtr)NativeConstants.SPI_SETWORKAREA, "WorkArea", 0x0002, 200, out dummy);
                }
                if (listView != IntPtr.Zero)
                {
                    NativeMethods.SendMessageTimeout(listView, NativeConstants.WM_SETTINGCHANGE, (IntPtr)NativeConstants.SPI_SETWORKAREA, "WorkArea", 0x0002, 200, out dummy);
                }

                // 2. Notify all top-level application windows (except visible primary Shell_TrayWnd)
                // so maximized windows on the affected monitor immediately resize to fill the full height!
                IntPtr primaryTray = NativeMethods.FindWindow("Shell_TrayWnd", null);
                NativeMethods.EnumWindows((hWnd, lParam) =>
                {
                    if (hWnd != primaryTray && NativeMethods.IsWindowVisible(hWnd))
                    {
                        NativeMethods.SendMessageTimeout(
                            hWnd,
                            NativeConstants.WM_SETTINGCHANGE,
                            (IntPtr)NativeConstants.SPI_SETWORKAREA,
                            "WorkArea",
                            0x0002,
                            50,
                            out _);
                    }
                    return true;
                }, IntPtr.Zero);
            });
        }
    }
}
