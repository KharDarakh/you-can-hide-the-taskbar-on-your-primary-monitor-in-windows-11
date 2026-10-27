using System;

namespace YCHTTOPYMIW11.Core.Models
{
    public sealed class MonitorPolicyConfig
    {
        public string DeviceName { get; set; } = string.Empty;
        public string FriendlyName { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public TaskbarPolicy Policy { get; set; } = TaskbarPolicy.AlwaysVisible;
        public bool CorrectDesktopWorkArea { get; set; } = true;

        public MonitorPolicyConfig Clone()
        {
            return new MonitorPolicyConfig
            {
                DeviceName = DeviceName,
                FriendlyName = FriendlyName,
                IsPrimary = IsPrimary,
                Policy = Policy,
                CorrectDesktopWorkArea = CorrectDesktopWorkArea
            };
        }
    }
}

