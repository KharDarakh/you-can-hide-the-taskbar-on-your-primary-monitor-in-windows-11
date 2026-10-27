using System;
using System.Collections.Generic;
using YCHTTOPYMIW11.Core.Models;

namespace YCHTTOPYMIW11.Core.Services
{
    public interface ITaskbarPolicyEngine : IDisposable
    {
        bool ApplyPolicy(DisplayInfo display, TaskbarPolicy policy, bool correctWorkArea);
        bool ApplyAllPolicies(IReadOnlyList<(DisplayInfo Display, TaskbarPolicy Policy, bool CorrectWorkArea)> policies);
        void RestoreAllPolicies();
        TaskbarPolicy GetCurrentPolicy(DisplayInfo display);
        IReadOnlyDictionary<string, TaskbarPolicy> GetAppliedPolicies();
    }
}

