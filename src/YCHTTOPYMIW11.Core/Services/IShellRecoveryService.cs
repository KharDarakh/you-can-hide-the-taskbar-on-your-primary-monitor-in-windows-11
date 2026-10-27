using System;

namespace YCHTTOPYMIW11.Core.Services
{
    public interface IShellRecoveryService : IDisposable
    {
        void StartListening();
        void StopListening();
        event EventHandler? ExplorerRestarted;
        event EventHandler? DisplayTopologyChanged;
    }
}

