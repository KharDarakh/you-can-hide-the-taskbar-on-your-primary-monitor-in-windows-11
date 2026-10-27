using YCHTTOPYMIW11.Diagnostics.Models;

namespace YCHTTOPYMIW11.Diagnostics.Services
{
    public interface ISystemReportGenerator
    {
        string GenerateSnapshotReport(DiagnosticSnapshot snapshot);
        string GenerateComparisonReport(DiagnosticComparison comparison);
    }
}

