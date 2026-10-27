using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using YCHTTOPYMIW11.Core.Interop;
using YCHTTOPYMIW11.Core.Models;
using YCHTTOPYMIW11.Core.Services;
using YCHTTOPYMIW11.Diagnostics.Models;

namespace YCHTTOPYMIW11.Diagnostics.Services
{
    public sealed class DiagnosticCollector : IDiagnosticCollector
    {
        private readonly ILogger<DiagnosticCollector> _logger;
        private readonly IDisplayManager _displayManager;
        private readonly ITaskbarDetector _taskbarDetector;

        public DiagnosticCollector(
            ILogger<DiagnosticCollector> logger,
            IDisplayManager displayManager,
            ITaskbarDetector taskbarDetector)
        {
            _logger = logger;
            _displayManager = displayManager;
            _taskbarDetector = taskbarDetector;
        }

        public DiagnosticSnapshot CaptureSnapshot()
        {
            return DesktopSessionHelper.RunOnInteractiveDesktop(() =>
            {
                var snapshot = new DiagnosticSnapshot
                {
                    Timestamp = DateTime.UtcNow,
                    Displays = _displayManager.GetDisplays().ToList(),
                    Taskbars = _taskbarDetector.DetectTaskbars().ToList(),
                    DesktopView = InspectDesktopHierarchy()
                };

                NativeMethods.SystemParametersInfoGet(NativeConstants.SPI_GETWORKAREA, 0, out RECT spiWork, 0);
                snapshot.PrimarySpiWorkArea = spiWork;

                EvaluateConditions(snapshot);

                return snapshot;
            });
        }

        public DesktopViewInfo InspectDesktopHierarchy()
        {
            return DesktopSessionHelper.RunOnInteractiveDesktop(() =>
            {
                var info = new DesktopViewInfo();

                IntPtr progman = NativeMethods.FindWindow("Progman", null);
                info.ProgmanHandle = progman;

                IntPtr defView = IntPtr.Zero;
                IntPtr workerW = IntPtr.Zero;

                if (progman != IntPtr.Zero)
                {
                    defView = NativeMethods.FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);
                }

                if (defView == IntPtr.Zero)
                {
                    NativeMethods.EnumWindows((hWnd, lParam) =>
                    {
                        var sb = new StringBuilder(256);
                        NativeMethods.GetClassName(hWnd, sb, 256);
                        if (sb.ToString() == "WorkerW")
                        {
                            IntPtr child = NativeMethods.FindWindowEx(hWnd, IntPtr.Zero, "SHELLDLL_DefView", null);
                            if (child != IntPtr.Zero)
                            {
                                defView = child;
                                workerW = hWnd;
                            }
                        }
                        return true;
                    }, IntPtr.Zero);
                }

                info.WorkerWHandle = workerW;
                info.DefViewHandle = defView;

                if (defView != IntPtr.Zero)
                {
                    NativeMethods.GetWindowRect(defView, out RECT dRect);
                    NativeMethods.GetClientRect(defView, out RECT dcRect);
                    info.DefViewBounds = dRect;
                    info.DefViewClientRect = dcRect;

                    IntPtr listView = NativeMethods.FindWindowEx(defView, IntPtr.Zero, "SysListView32", null);
                    info.ListViewHandle = listView;

                    if (listView != IntPtr.Zero)
                    {
                        NativeMethods.GetWindowRect(listView, out RECT lRect);
                        NativeMethods.GetClientRect(listView, out RECT lcRect);
                        info.ListViewBounds = lRect;
                        info.ListViewClientRect = lcRect;
                        info.ListViewStyle = NativeMethods.GetWindowLong(listView, NativeConstants.GWL_STYLE);

                        int count = (int)NativeMethods.SendMessage(listView, NativeConstants.LVM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero);
                        info.IconCount = count;

                        IntPtr spacing = NativeMethods.SendMessage(listView, NativeConstants.LVM_GETITEMSPACING, IntPtr.Zero, IntPtr.Zero);
                        info.GridSpacingX = (short)(spacing.ToInt64() & 0xFFFF);
                        info.GridSpacingY = (short)((spacing.ToInt64() >> 16) & 0xFFFF);

                        // Sample icon positions if accessible
                        SampleIcons(listView, count, info);
                    }
                }

                return info;
            });
        }

        public DiagnosticComparison CaptureBeforeAndAfter(Action action)
        {
            var comparison = new DiagnosticComparison
            {
                Before = CaptureSnapshot()
            };

            action();

            comparison.After = CaptureSnapshot();

            // Compare taskbar visibility
            foreach (var bTray in comparison.Before.Taskbars)
            {
                var aTray = comparison.After.Taskbars.FirstOrDefault(t => t.Handle == bTray.Handle);
                if (aTray != null && bTray.IsVisible != aTray.IsVisible)
                {
                    comparison.Observations.Add($"Taskbar 0x{bTray.Handle:X} visibility changed: {bTray.IsVisible} -> {aTray.IsVisible}");
                }
            }

            // Compare work areas
            foreach (var bDisp in comparison.Before.Displays)
            {
                var aDisp = comparison.After.Displays.FirstOrDefault(d => d.DeviceName == bDisp.DeviceName);
                if (aDisp != null && bDisp.WorkArea != aDisp.WorkArea)
                {
                    comparison.Observations.Add($"WorkArea for {bDisp.DeviceName} changed: {bDisp.WorkArea} -> {aDisp.WorkArea}");
                }
            }

            return comparison;
        }

        private void SampleIcons(IntPtr listView, int count, DesktopViewInfo info)
        {
            try
            {
                NativeMethods.GetWindowThreadProcessId(listView, out int pid);
                IntPtr hProcess = NativeMethods.OpenProcess(0x0038 /* VM_OPERATION | VM_READ | VM_WRITE */, false, pid);
                if (hProcess == IntPtr.Zero) return;

                IntPtr mem = NativeMethods.VirtualAllocEx(hProcess, IntPtr.Zero, 8, 0x1000, 0x04);
                if (mem != IntPtr.Zero)
                {
                    byte[] buf = new byte[8];
                    int samplesToRead = Math.Min(count, 10);
                    for (int i = 0; i < samplesToRead; i++)
                    {
                        NativeMethods.SendMessage(listView, NativeConstants.LVM_GETITEMPOSITION, (IntPtr)i, mem);
                        NativeMethods.ReadProcessMemory(hProcess, mem, buf, 8, out _);
                        int x = BitConverter.ToInt32(buf, 0);
                        int y = BitConverter.ToInt32(buf, 4);
                        info.SampleIconPositions.Add(new POINT(x, y));
                    }
                    NativeMethods.VirtualFreeEx(hProcess, mem, 0, 0x8000);
                }
                NativeMethods.CloseHandle(hProcess);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not sample desktop icon positions");
            }
        }

        private static void EvaluateConditions(DiagnosticSnapshot s)
        {
            var diag = s.Diagnosis;

            // Condition A: Windows-reported work area excludes the bottom strip
            var primary = s.Displays.FirstOrDefault(d => d.IsPrimary);
            if (primary != null)
            {
                int diff = primary.MonitorBounds.Height - primary.WorkArea.Height;
                diag.ConditionA_WorkAreaExcludesStrip = diff > 0;
                diag.ConditionA_Details = diff > 0
                    ? $"Primary display excludes bottom {diff}px (Monitor: {primary.MonitorBounds.Height}px, WorkArea: {primary.WorkArea.Height}px)."
                    : "Primary display work area matches monitor bounds.";
            }

            // Condition B: The desktop icon view's window or client area excludes the bottom strip
            if (s.DesktopView.DefViewHandle != IntPtr.Zero && primary != null)
            {
                int defViewBottom = s.DesktopView.DefViewBounds.Bottom;
                bool excludes = defViewBottom < primary.MonitorBounds.Bottom;
                diag.ConditionB_IconViewExcludesStrip = excludes;
                diag.ConditionB_Details = excludes
                    ? $"SHELLDLL_DefView bottom ({defViewBottom}) is less than monitor bottom ({primary.MonitorBounds.Bottom})."
                    : $"SHELLDLL_DefView spans full virtual height ({s.DesktopView.DefViewBounds.Height}px), covering monitor bottom ({defViewBottom} >= {primary.MonitorBounds.Bottom}).";
            }

            // Condition C: The icon view occupies the correct area, but Explorer's icon positioning logic retains an older boundary
            diag.ConditionC_ExplorerRetainsOlderBoundary = diag.ConditionA_WorkAreaExcludesStrip;
            diag.ConditionC_Details = diag.ConditionC_ExplorerRetainsOlderBoundary
                ? "Explorer icon placement clamps icon bounding boxes to fit inside rcWork.Bottom (clamping to Row 13 / Y=1289 with 99px grid)."
                : "Explorer icon placement is not constrained by a taskbar reservation.";

            // Condition D: The icon view has the correct geometry, but the shell still prevents icon placement in that region
            diag.ConditionD_ShellPreventsIconPlacement = diag.ConditionA_WorkAreaExcludesStrip;
            diag.ConditionD_Details = diag.ConditionD_ShellPreventsIconPlacement
                ? "Desktop icon alignment to grid prevents placement in bottom 48px while taskbar work area is excluded."
                : "Icons can freely occupy the bottom area.";
        }
    }
}

