using System;
using System.Collections.Generic;

namespace YCHTTOPYMIW11.Core.Models
{
    public sealed class AppConfig
    {
        public int Version { get; set; } = 1;
        public List<MonitorPolicyConfig> MonitorPolicies { get; set; } = new List<MonitorPolicyConfig>();
        public bool StartWithWindows { get; set; } = false;
        public bool StartMinimizedToTray { get; set; } = false;
        public bool EnableDiagnosticLogging { get; set; } = true;
        public bool AutoRecoverOnExplorerRestart { get; set; } = true;
        public bool AutoRecoverOnDisplayChange { get; set; } = true;
        public bool RestoreOnExit { get; set; } = true;
    }
}

