using System;
using System.Collections.Generic;
using YCHTTOPYMIW11.Core.Models;

namespace YCHTTOPYMIW11.Core.Services
{
    public interface IDisplayManager
    {
        IReadOnlyList<DisplayInfo> GetDisplays();
        DisplayInfo? GetPrimaryDisplay();
        DisplayInfo? FindDisplayByDeviceName(string deviceName);
        DisplayInfo? FindDisplayByHandle(IntPtr hMonitor);
    }
}

