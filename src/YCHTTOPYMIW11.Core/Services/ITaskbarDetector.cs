using System;
using System.Collections.Generic;
using YCHTTOPYMIW11.Core.Models;

namespace YCHTTOPYMIW11.Core.Services
{
    public interface ITaskbarDetector
    {
        IReadOnlyList<TaskbarWindowInfo> DetectTaskbars();
        TaskbarWindowInfo? GetPrimaryTaskbar();
        TaskbarWindowInfo? GetTaskbarForDisplay(DisplayInfo display);
    }
}

