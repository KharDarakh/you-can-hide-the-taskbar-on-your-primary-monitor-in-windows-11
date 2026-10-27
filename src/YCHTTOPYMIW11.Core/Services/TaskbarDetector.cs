using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using YCHTTOPYMIW11.Core.Interop;
using YCHTTOPYMIW11.Core.Models;

namespace YCHTTOPYMIW11.Core.Services
{
    public sealed class TaskbarDetector : ITaskbarDetector
    {
        private readonly ILogger<TaskbarDetector> _logger;
        private readonly IDisplayManager _displayManager;

        public TaskbarDetector(ILogger<TaskbarDetector> logger, IDisplayManager displayManager)
        {
            _logger = logger;
            _displayManager = displayManager;
        }

        public IReadOnlyList<TaskbarWindowInfo> DetectTaskbars()
        {
            return DesktopSessionHelper.RunOnInteractiveDesktop(() =>
            {
                var list = new List<TaskbarWindowInfo>();
                var displays = _displayManager.GetDisplays();

                // 1. Primary Taskbar: Shell_TrayWnd
                IntPtr primaryTray = NativeMethods.FindWindow("Shell_TrayWnd", null);
                if (primaryTray != IntPtr.Zero)
                {
                    var info = InspectWindow(primaryTray, "Shell_TrayWnd", TaskbarRole.Primary, displays);
                    list.Add(info);
                }

                // 2. Secondary Taskbars: Shell_SecondaryTrayWnd (enumerate all instances)
                NativeMethods.EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
                {
                    var sb = new StringBuilder(256);
                    NativeMethods.GetClassName(hWnd, sb, 256);
                    string cls = sb.ToString();

                    if (cls.Equals("Shell_SecondaryTrayWnd", StringComparison.OrdinalIgnoreCase))
                    {
                        var secInfo = InspectWindow(hWnd, "Shell_SecondaryTrayWnd", TaskbarRole.Secondary, displays);
                        list.Add(secInfo);
                    }
                    return true;
                }, IntPtr.Zero);

                _logger.LogDebug("Detected {Count} taskbar window(s)", list.Count);
                return list;
            });
        }

        public TaskbarWindowInfo? GetPrimaryTaskbar()
        {
            return DetectTaskbars().FirstOrDefault(t => t.Role == TaskbarRole.Primary);
        }

        public TaskbarWindowInfo? GetTaskbarForDisplay(DisplayInfo display)
        {
            var taskbars = DetectTaskbars();

            // If display is primary, it ALWAYS owns Shell_TrayWnd
            if (display.IsPrimary)
            {
                var prim = taskbars.FirstOrDefault(t => t.Role == TaskbarRole.Primary);
                if (prim != null) return prim;
            }

            // Match by MonitorHandle
            var matched = taskbars.FirstOrDefault(t => t.MonitorHandle == display.MonitorHandle);
            if (matched != null) return matched;

            // Match by intersection of Bounds with MonitorBounds (for on-screen taskbars)
            foreach (var t in taskbars)
            {
                if (IsRectInside(t.Bounds, display.MonitorBounds))
                {
                    return t;
                }
            }

            // Fallback for secondary displays: return the secondary taskbar
            if (!display.IsPrimary)
            {
                return taskbars.FirstOrDefault(t => t.Role == TaskbarRole.Secondary);
            }

            return null;
        }

        private TaskbarWindowInfo InspectWindow(IntPtr hWnd, string className, TaskbarRole role, IReadOnlyList<DisplayInfo> displays)
        {
            NativeMethods.GetWindowRect(hWnd, out RECT rect);
            bool isVisible = NativeMethods.IsWindowVisible(hWnd);
            IntPtr hMon = NativeMethods.MonitorFromWindow(hWnd, NativeConstants.MONITOR_DEFAULTTONEAREST);
            int style = NativeMethods.GetWindowLong(hWnd, NativeConstants.GWL_STYLE);
            int exStyle = NativeMethods.GetWindowLong(hWnd, NativeConstants.GWL_EXSTYLE);

            var owningDisplay = displays.FirstOrDefault(d => d.MonitorHandle == hMon);

            return new TaskbarWindowInfo
            {
                Handle = hWnd,
                ClassName = className,
                Role = role,
                MonitorHandle = hMon,
                MonitorDeviceName = owningDisplay?.DeviceName ?? string.Empty,
                Bounds = rect,
                OriginalBounds = rect,
                IsVisible = isVisible,
                Style = style,
                ExtendedStyle = exStyle
            };
        }

        private static bool IsRectInside(RECT inner, RECT outer)
        {
            int centerX = inner.Left + inner.Width / 2;
            int centerY = inner.Top + inner.Height / 2;
            return centerX >= outer.Left && centerX <= outer.Right &&
                   centerY >= outer.Top && centerY <= outer.Bottom;
        }
    }
}
