using System;
using System.Collections.Generic;

namespace YCHTTOPYMIW11.Core.Models
{
    public enum CloseAction
    {
        Ask = 0,
        MinimizeToTray = 1,
        Exit = 2
    }

    public sealed class AppConfig
    {
        public int Version { get; set; } = 1;
        public string Language { get; set; } = string.Empty;
        public CloseAction CloseAction { get; set; } = CloseAction.Ask;
        public List<MonitorPolicyConfig> MonitorPolicies { get; set; } = new List<MonitorPolicyConfig>();
        public bool StartWithWindows { get; set; } = false;
        public bool StartMinimizedToTray { get; set; } = false;
        public bool EnableDiagnosticLogging { get; set; } = true;
        public bool AutoRecoverOnExplorerRestart { get; set; } = true;
        public bool AutoRecoverOnDisplayChange { get; set; } = true;
        public bool RestoreOnExit { get; set; } = true;
    }
}
