using System;
using System.Collections.Generic;
using Xunit;
using YCHTTOPYMIW11.Core.Interop;
using YCHTTOPYMIW11.Core.Models;
using YCHTTOPYMIW11.Diagnostics.Models;
using YCHTTOPYMIW11.Diagnostics.Services;

namespace YCHTTOPYMIW11.Tests
{
    public class DiagnosticReportTests
    {
        [Fact]
        public void SystemReportGenerator_ProducesComprehensiveReport()
        {
            var generator = new SystemReportGenerator();
            var snapshot = new DiagnosticSnapshot
            {
                Timestamp = new DateTime(2026, 10, 27, 4, 0, 0, DateTimeKind.Utc),
                Displays = new List<DisplayInfo>
                {
                    new DisplayInfo
                    {
                        DeviceName = @"\\.\DISPLAY1",
                        FriendlyName = "DISPLAY1 (Ana Ekran)",
                        IsPrimary = true,
                        MonitorBounds = new RECT(0, 0, 2560, 1440),
                        WorkArea = new RECT(0, 0, 2560, 1392),
                        DpiX = 96,
                        DpiY = 96
                    },
                    new DisplayInfo
                    {
                        DeviceName = @"\\.\DISPLAY2",
                        FriendlyName = "DISPLAY2 (İkincil Ekran)",
                        IsPrimary = false,
                        MonitorBounds = new RECT(-1920, 0, 0, 1080),
                        WorkArea = new RECT(-1920, 0, 0, 1032),
                        DpiX = 96,
                        DpiY = 96
                    }
                },
                Taskbars = new List<TaskbarWindowInfo>
                {
                    new TaskbarWindowInfo
                    {
                        Handle = new IntPtr(0x100DE),
                        ClassName = "Shell_TrayWnd",
                        Role = TaskbarRole.Primary,
                        MonitorDeviceName = @"\\.\DISPLAY1",
                        Bounds = new RECT(0, 1392, 2560, 1440),
                        IsVisible = true,
                        Style = unchecked((int)0x96000000),
                        ExtendedStyle = 0x00000088
                    },
                    new TaskbarWindowInfo
                    {
                        Handle = new IntPtr(0x6098C),
                        ClassName = "Shell_SecondaryTrayWnd",
                        Role = TaskbarRole.Secondary,
                        MonitorDeviceName = @"\\.\DISPLAY2",
                        Bounds = new RECT(-1920, 1032, 0, 1080),
                        IsVisible = true,
                        Style = unchecked((int)0x96000000),
                        ExtendedStyle = 0x00000088
                    }
                },
                DesktopView = new DesktopViewInfo
                {
                    ProgmanHandle = new IntPtr(0x10106),
                    DefViewHandle = new IntPtr(0x1010A),
                    DefViewBounds = new RECT(-1920, 0, 2560, 1440),
                    ListViewHandle = new IntPtr(0x1010E),
                    ListViewBounds = new RECT(-1920, 0, 2560, 1440),
                    IconCount = 7,
                    GridSpacingX = 75,
                    GridSpacingY = 99
                },
                Diagnosis = new ConditionDiagnosis
                {
                    ConditionA_WorkAreaExcludesStrip = true,
                    ConditionA_Details = "Primary display excludes bottom 48px.",
                    ConditionB_IconViewExcludesStrip = false,
                    ConditionB_Details = "SHELLDLL_DefView covers entire virtual screen.",
                    ConditionC_ExplorerRetainsOlderBoundary = true,
                    ConditionC_Details = "Explorer clamps icons to rcWork bottom.",
                    ConditionD_ShellPreventsIconPlacement = true,
                    ConditionD_Details = "Grid alignment prevents placement in bottom 48px."
                }
            };

            string report = generator.GenerateSnapshotReport(snapshot);

            Assert.Contains("YCHTTOPYMIW11 Windows 11 Sistem Tanılama Raporu", report);
            Assert.Contains("DISPLAY1 (Ana Ekran)", report);
            Assert.Contains("DISPLAY2 (İkincil Ekran)", report);
            Assert.Contains("Shell_TrayWnd", report);
            Assert.Contains("Shell_SecondaryTrayWnd", report);
            Assert.Contains("SHELLDLL_DefView", report);
            Assert.Contains("Durum A (Çalışma Alanı Kısıtlaması)", report);
            Assert.Contains("Durum B (Masaüstü Pencere Kısıtlaması)", report);
            Assert.Contains("Durum C", report);
            Assert.Contains("Durum D", report);
        }

        [Fact]
        public void SystemReportGenerator_ProducesComparisonReport()
        {
            var generator = new SystemReportGenerator();
            var comparison = new DiagnosticComparison
            {
                Observations = new List<string>
                {
                    "Taskbar 0x100DE visibility changed: True -> False",
                    "WorkArea for \\\\.\\DISPLAY1 changed: (0, 0, 2560, 1392) -> (0, 0, 2560, 1440)"
                }
            };

            string report = generator.GenerateComparisonReport(comparison);

            Assert.Contains("YCHTTOPYMIW11 Öncesi ve Sonrası Karşılaştırma Raporu", report);
            Assert.Contains("Gözlemlenen Değişiklikler", report);
            Assert.Contains("Taskbar 0x100DE visibility changed", report);
            Assert.Contains("WorkArea for \\\\.\\DISPLAY1 changed", report);
        }
    }
}
