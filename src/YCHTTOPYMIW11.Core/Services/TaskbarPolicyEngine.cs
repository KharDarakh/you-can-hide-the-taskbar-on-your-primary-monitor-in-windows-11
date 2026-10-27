using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using YCHTTOPYMIW11.Core.Interop;
using YCHTTOPYMIW11.Core.Models;

namespace YCHTTOPYMIW11.Core.Services
{
    public sealed class TaskbarPolicyEngine : ITaskbarPolicyEngine
    {
        private readonly ILogger<TaskbarPolicyEngine> _logger;
        private readonly ITaskbarDetector _taskbarDetector;
        private readonly IWorkAreaManager _workAreaManager;
        private readonly ConcurrentDictionary<string, TaskbarPolicy> _appliedPolicies = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, RECT> _originalBounds = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, IntPtr> _pinnedTaskbarHandles = new(StringComparer.OrdinalIgnoreCase);
        private bool _disposed;

        public TaskbarPolicyEngine(
            ILogger<TaskbarPolicyEngine> logger,
            ITaskbarDetector taskbarDetector,
            IWorkAreaManager workAreaManager)
        {
            _logger = logger;
            _taskbarDetector = taskbarDetector;
            _workAreaManager = workAreaManager;
        }

        public bool ApplyPolicy(DisplayInfo display, TaskbarPolicy policy, bool correctWorkArea)
        {
            return ApplyAllPolicies(new[] { (display, policy, correctWorkArea) });
        }

        public bool ApplyAllPolicies(IReadOnlyList<(DisplayInfo Display, TaskbarPolicy Policy, bool CorrectWorkArea)> policies)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(TaskbarPolicyEngine));
            if (policies == null || policies.Count == 0) return true;

            return DesktopSessionHelper.RunOnInteractiveDesktop(() =>
            {
                var targets = new List<(DisplayInfo Display, TaskbarPolicy Policy, bool CorrectWorkArea, IntPtr Hwnd)>();

                // 1. Resolve taskbar HWNDs and cache bounds
                foreach (var item in policies)
                {
                    IntPtr taskbarHwnd = IntPtr.Zero;
                    if (_pinnedTaskbarHandles.TryGetValue(item.Display.DeviceName, out IntPtr pinned) && NativeMethods.IsWindow(pinned))
                    {
                        taskbarHwnd = pinned;
                    }
                    else
                    {
                        var taskbar = _taskbarDetector.GetTaskbarForDisplay(item.Display);
                        if (taskbar != null)
                        {
                            taskbarHwnd = taskbar.Handle;
                            _pinnedTaskbarHandles[item.Display.DeviceName] = taskbarHwnd;
                        }
                    }

                    if (taskbarHwnd == IntPtr.Zero || !NativeMethods.IsWindow(taskbarHwnd))
                    {
                        _logger.LogWarning("No taskbar window found for display {Device}", item.Display.DeviceName);
                        continue;
                    }

                    NativeMethods.GetWindowRect(taskbarHwnd, out RECT currentBounds);
                    if (!_originalBounds.ContainsKey(item.Display.DeviceName) && currentBounds.Top < 10000)
                    {
                        _originalBounds[item.Display.DeviceName] = currentBounds;
                    }

                    targets.Add((item.Display, item.Policy, item.CorrectWorkArea, taskbarHwnd));
                }

                // 2. Ensure global shell auto-hide is disabled so Explorer operates in standard AlwaysOnTop mode
                IntPtr primaryTray = NativeMethods.FindWindow("Shell_TrayWnd", null);
                if (primaryTray != IntPtr.Zero)
                {
                    var abd = APPBARDATA.Create(primaryTray);
                    abd.lParam = (IntPtr)NativeConstants.ABS_ALWAYSONTOP;
                    NativeMethods.SHAppBarMessage(NativeConstants.ABM_SETSTATE, ref abd);
                }

                // 3. Apply window positions and visibility for each target
                foreach (var target in targets)
                {
                    _logger.LogInformation("Applying policy {Policy} to display {Device} (Taskbar HWND: 0x{Handle:X})",
                        target.Policy, target.Display.DeviceName, target.Hwnd.ToInt64());

                    switch (target.Policy)
                    {
                        case TaskbarPolicy.PermanentlyHidden:
                            // Hide window and park far off-screen
                            NativeMethods.ShowWindow(target.Hwnd, NativeConstants.SW_HIDE);
                            NativeMethods.SetWindowPos(
                                target.Hwnd,
                                IntPtr.Zero,
                                0,
                                30000,
                                0,
                                0,
                                NativeConstants.SWP_NOSIZE | NativeConstants.SWP_NOZORDER |
                                NativeConstants.SWP_NOACTIVATE | NativeConstants.SWP_HIDEWINDOW);
                            break;

                        case TaskbarPolicy.AlwaysVisible:
                        default:
                            // Restore window position and show window
                            RECT targetBounds;
                            if (!_originalBounds.TryGetValue(target.Display.DeviceName, out targetBounds) || targetBounds.Top >= 10000)
                            {
                                targetBounds = new RECT(
                                    target.Display.MonitorBounds.Left,
                                    target.Display.MonitorBounds.Bottom - 48,
                                    target.Display.MonitorBounds.Right,
                                    target.Display.MonitorBounds.Bottom);
                            }

                            NativeMethods.SetWindowPos(
                                target.Hwnd,
                                NativeConstants.HWND_TOP,
                                targetBounds.Left,
                                targetBounds.Top,
                                targetBounds.Width,
                                targetBounds.Height,
                                NativeConstants.SWP_NOACTIVATE | NativeConstants.SWP_SHOWWINDOW);

                            NativeMethods.ShowWindow(target.Hwnd, NativeConstants.SW_SHOW);
                            break;
                    }

                    _appliedPolicies[target.Display.DeviceName] = target.Policy;
                }

                // 4. Apply work areas (executed AFTER ABM_SETSTATE so Explorer does not revert)
                foreach (var target in targets)
                {
                    if (target.Policy == TaskbarPolicy.PermanentlyHidden)
                    {
                        _workAreaManager.ExpandWorkAreaToFull(target.Display);
                    }
                    else
                    {
                        _workAreaManager.RestoreWorkArea(target.Display);
                    }
                }

                return true;
            });
        }

        public void RestoreAllPolicies()
        {
            DesktopSessionHelper.RunOnInteractiveDesktop(() =>
            {
                _logger.LogInformation("Restoring all taskbar policies to system defaults...");

                var taskbars = _taskbarDetector.DetectTaskbars();
                foreach (var taskbar in taskbars)
                {
                    // Reset to AlwaysOnTop / Manual
                    var abd = APPBARDATA.Create(taskbar.Handle);
                    abd.lParam = (IntPtr)NativeConstants.ABS_ALWAYSONTOP;
                    NativeMethods.SHAppBarMessage(NativeConstants.ABM_SETSTATE, ref abd);

                    NativeMethods.ShowWindow(taskbar.Handle, NativeConstants.SW_SHOW);
                }

                foreach (var kvp in _originalBounds)
                {
                    string device = kvp.Key;
                    RECT orig = kvp.Value;
                    if (orig.Top >= 10000) continue; // Skip invalid off-screen bounds

                    IntPtr taskbarHwnd = IntPtr.Zero;
                    if (_pinnedTaskbarHandles.TryGetValue(device, out IntPtr pinned) && NativeMethods.IsWindow(pinned))
                    {
                        taskbarHwnd = pinned;
                    }
                    else
                    {
                        var taskbar = taskbars.FirstOrDefault(t => t.MonitorDeviceName.Equals(device, StringComparison.OrdinalIgnoreCase));
                        if (taskbar != null) taskbarHwnd = taskbar.Handle;
                    }

                    if (taskbarHwnd != IntPtr.Zero)
                    {
                        NativeMethods.SetWindowPos(
                            taskbarHwnd,
                            NativeConstants.HWND_TOP,
                            orig.Left,
                            orig.Top,
                            orig.Width,
                            orig.Height,
                            NativeConstants.SWP_NOACTIVATE | NativeConstants.SWP_SHOWWINDOW);
                        NativeMethods.ShowWindow(taskbarHwnd, NativeConstants.SW_SHOW);
                    }
                }

                _workAreaManager.RestoreAllWorkAreas();
                _appliedPolicies.Clear();
                _originalBounds.Clear();
                _pinnedTaskbarHandles.Clear();
            });
        }

        public TaskbarPolicy GetCurrentPolicy(DisplayInfo display)
        {
            if (_appliedPolicies.TryGetValue(display.DeviceName, out var policy))
            {
                return policy;
            }

            // Check pinned or detected taskbar
            IntPtr taskbarHwnd = IntPtr.Zero;
            if (_pinnedTaskbarHandles.TryGetValue(display.DeviceName, out IntPtr pinned) && NativeMethods.IsWindow(pinned))
            {
                taskbarHwnd = pinned;
            }
            else
            {
                var taskbar = _taskbarDetector.GetTaskbarForDisplay(display);
                if (taskbar != null) taskbarHwnd = taskbar.Handle;
            }

            if (taskbarHwnd == IntPtr.Zero || !NativeMethods.IsWindowVisible(taskbarHwnd))
            {
                return TaskbarPolicy.PermanentlyHidden;
            }

            // Check if Windows Explorer has AutoHide enabled for this taskbar
            var abd = APPBARDATA.Create(taskbarHwnd);
            UIntPtr state = NativeMethods.SHAppBarMessage(NativeConstants.ABM_GETSTATE, ref abd);
            if ((state.ToUInt64() & NativeConstants.ABS_AUTOHIDE) != 0)
            {
                return TaskbarPolicy.AutoHidden;
            }

            return TaskbarPolicy.AlwaysVisible;
        }

        public IReadOnlyDictionary<string, TaskbarPolicy> GetAppliedPolicies()
        {
            return _appliedPolicies;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                RestoreAllPolicies();
            }
        }
    }
}
