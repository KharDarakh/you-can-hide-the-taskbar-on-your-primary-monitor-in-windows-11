using System;
using System.Collections.Generic;
using YCHTTOPYMIW11.Core.Interop;
using YCHTTOPYMIW11.Core.Models;

namespace YCHTTOPYMIW11.Diagnostics.Models
{
    public sealed class DiagnosticSnapshot
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public List<DisplayInfo> Displays { get; set; } = new List<DisplayInfo>();
        public List<TaskbarWindowInfo> Taskbars { get; set; } = new List<TaskbarWindowInfo>();
        public DesktopViewInfo DesktopView { get; set; } = new DesktopViewInfo();
        public RECT PrimarySpiWorkArea { get; set; }

        public ConditionDiagnosis Diagnosis { get; set; } = new ConditionDiagnosis();
    }

    public sealed class ConditionDiagnosis
    {
        public bool ConditionA_WorkAreaExcludesStrip { get; set; }
        public string ConditionA_Details { get; set; } = string.Empty;

        public bool ConditionB_IconViewExcludesStrip { get; set; }
        public string ConditionB_Details { get; set; } = string.Empty;

        public bool ConditionC_ExplorerRetainsOlderBoundary { get; set; }
        public string ConditionC_Details { get; set; } = string.Empty;

        public bool ConditionD_ShellPreventsIconPlacement { get; set; }
        public string ConditionD_Details { get; set; } = string.Empty;
    }

    public sealed class DiagnosticComparison
    {
        public DiagnosticSnapshot Before { get; set; } = new DiagnosticSnapshot();
        public DiagnosticSnapshot After { get; set; } = new DiagnosticSnapshot();
        public List<string> Observations { get; set; } = new List<string>();
    }
}

