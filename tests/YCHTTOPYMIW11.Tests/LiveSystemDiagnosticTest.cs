using System;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using YCHTTOPYMIW11.Core.Services;
using YCHTTOPYMIW11.Diagnostics.Services;

namespace YCHTTOPYMIW11.Tests
{
    public class LiveSystemDiagnosticTest
    {
        [Fact]
        public void DiagnosticCollector_CapturesLiveSystemAndGeneratesReport()
        {
            var displayManager = new DisplayManager(NullLogger<DisplayManager>.Instance);
            var taskbarDetector = new TaskbarDetector(NullLogger<TaskbarDetector>.Instance, displayManager);
            var collector = new DiagnosticCollector(NullLogger<DiagnosticCollector>.Instance, displayManager, taskbarDetector);
            var reportGenerator = new SystemReportGenerator();

            var snapshot = collector.CaptureSnapshot();

            Assert.NotNull(snapshot);
            Assert.NotEmpty(snapshot.Displays);
            Assert.NotNull(snapshot.DesktopView);

            string report = reportGenerator.GenerateSnapshotReport(snapshot);
            Assert.NotEmpty(report);

            // Write the diagnostic report to docs/diagnostic-system-report.md if docs directory exists
            var dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "YCHTTOPYMIW11.sln")))
            {
                dir = dir.Parent;
            }

            if (dir != null)
            {
                string docsDir = Path.Combine(dir.FullName, "docs");
                if (Directory.Exists(docsDir))
                {
                    File.WriteAllText(Path.Combine(docsDir, "diagnostic-system-report.md"), report);
                }
            }
        }
    }
}
