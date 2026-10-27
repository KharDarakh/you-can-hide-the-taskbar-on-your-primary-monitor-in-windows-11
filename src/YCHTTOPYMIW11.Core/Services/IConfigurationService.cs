using YCHTTOPYMIW11.Core.Models;

namespace YCHTTOPYMIW11.Core.Services
{
    public interface IConfigurationService
    {
        AppConfig LoadConfig();
        void SaveConfig(AppConfig config);
        void ResetToDefaults();
    }
}

