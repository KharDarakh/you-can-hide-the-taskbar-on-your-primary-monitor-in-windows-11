using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using YCHTTOPYMIW11.Core.Interop;
using YCHTTOPYMIW11.Core.Models;

namespace YCHTTOPYMIW11.Core.Services
{
    public sealed class DisplayManager : IDisplayManager
    {
        private readonly ILogger<DisplayManager> _logger;

        public DisplayManager(ILogger<DisplayManager> logger)
        {
            _logger = logger;
        }

        public IReadOnlyList<DisplayInfo> GetDisplays()
        {
            return DesktopSessionHelper.RunOnInteractiveDesktop(() =>
            {
                var displays = new List<DisplayInfo>();

                NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
                {
                    var mi = MONITORINFOEX.Create();
                    if (NativeMethods.GetMonitorInfo(hMonitor, ref mi))
                    {
                        bool isPrimary = (mi.dwFlags & NativeConstants.MONITORINFOF_PRIMARY) != 0;
                        string device = mi.szDevice ?? string.Empty;

                        uint dpiX = 96, dpiY = 96;
                        try
                        {
                            NativeMethods.GetDpiForMonitor(hMonitor, 0 /* MDT_EFFECTIVE_DPI */, out dpiX, out dpiY);
                        }
                        catch
                        {
                            dpiX = 96;
                            dpiY = 96;
                        }

                        var display = new DisplayInfo
                        {
                            MonitorHandle = hMonitor,
                            DeviceName = device,
                            FriendlyName = GetFriendlyName(device, isPrimary),
                            IsPrimary = isPrimary,
                            MonitorBounds = mi.rcMonitor,
                            WorkArea = mi.rcWork,
                            DpiX = dpiX,
                            DpiY = dpiY
                        };

                        displays.Add(display);
                    }
                    return true;
                }, IntPtr.Zero);

                _logger.LogDebug("Enumerated {Count} display(s)", displays.Count);
                return displays;
            });
        }

        public DisplayInfo? GetPrimaryDisplay()
        {
            return GetDisplays().FirstOrDefault(d => d.IsPrimary);
        }

        public DisplayInfo? FindDisplayByDeviceName(string deviceName)
        {
            return GetDisplays().FirstOrDefault(d => d.DeviceName.Equals(deviceName, StringComparison.OrdinalIgnoreCase));
        }

        public DisplayInfo? FindDisplayByHandle(IntPtr hMonitor)
        {
            return GetDisplays().FirstOrDefault(d => d.MonitorHandle == hMonitor);
        }

        private static string GetFriendlyName(string deviceName, bool isPrimary)
        {
            string clean = deviceName.Replace(@"\\.\", "");
            return isPrimary ? $"{clean} (Ana Ekran)" : $"{clean} (İkincil Ekran)";
        }
    }
}

