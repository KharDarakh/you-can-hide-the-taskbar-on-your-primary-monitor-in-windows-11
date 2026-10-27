using System;
using YCHTTOPYMIW11.Core.Models;
using YCHTTOPYMIW11.Diagnostics.Models;

namespace YCHTTOPYMIW11.Diagnostics.Services
{
    public interface IDiagnosticCollector
    {
        DiagnosticSnapshot CaptureSnapshot();
        DesktopViewInfo InspectDesktopHierarchy();
        DiagnosticComparison CaptureBeforeAndAfter(Action action);
    }
}

