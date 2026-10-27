using System;
using YCHTTOPYMIW11.Core.Interop;
using YCHTTOPYMIW11.Core.Models;

namespace YCHTTOPYMIW11.Core.Services
{
    public interface IWorkAreaManager
    {
        RECT GetCurrentWorkArea(DisplayInfo display);
        bool ExpandWorkAreaToFull(DisplayInfo display);
        bool RestoreWorkArea(DisplayInfo display);
        void RestoreAllWorkAreas();
        void RefreshDesktopLayout();
    }
}

