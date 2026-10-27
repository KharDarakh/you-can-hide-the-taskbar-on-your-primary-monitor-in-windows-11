using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using YCHTTOPYMIW11.Core.Models;

namespace YCHTTOPYMIW11.Core.Services
{
    public sealed class ConfigurationService : IConfigurationService
    {
        private readonly ILogger<ConfigurationService> _logger;
        private readonly string _configDirectory;
        private readonly string _configFilePath;
        private readonly string _backupFilePath;
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public ConfigurationService(ILogger<ConfigurationService> logger, string? configDirectory = null)
        {
            _logger = logger;
            _configDirectory = configDirectory ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "YCHTTOPYMIW11");
            _configFilePath = Path.Combine(_configDirectory, "config.json");
            _backupFilePath = Path.Combine(_configDirectory, "config.backup.json");
        }

        public AppConfig LoadConfig()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    string json = File.ReadAllText(_configFilePath);
                    var config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions);
                    if (config != null)
                    {
                        _logger.LogInformation("Configuration loaded from {Path} (Version: {Version})", _configFilePath, config.Version);
                        return config;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load config from {Path}. Attempting backup...", _configFilePath);
                try
                {
                    if (File.Exists(_backupFilePath))
                    {
                        string bkpJson = File.ReadAllText(_backupFilePath);
                        var bkpConfig = JsonSerializer.Deserialize<AppConfig>(bkpJson, JsonOptions);
                        if (bkpConfig != null)
                        {
                            _logger.LogInformation("Restored configuration from backup {Path}", _backupFilePath);
                            return bkpConfig;
                        }
                    }
                }
                catch (Exception bkpEx)
                {
                    _logger.LogError(bkpEx, "Failed to load backup configuration");
                }
            }

            _logger.LogInformation("Returning default configuration");
            return new AppConfig();
        }

        public void SaveConfig(AppConfig config)
        {
            try
            {
                Directory.CreateDirectory(_configDirectory);

                // Create backup of current config if exists
                if (File.Exists(_configFilePath))
                {
                    File.Copy(_configFilePath, _backupFilePath, true);
                }

                string json = JsonSerializer.Serialize(config, JsonOptions);
                File.WriteAllText(_configFilePath, json);
                _logger.LogInformation("Configuration saved to {Path}", _configFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save configuration to {Path}", _configFilePath);
            }
        }

        public void ResetToDefaults()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    File.Delete(_configFilePath);
                }
                _logger.LogInformation("Configuration reset to defaults");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reset configuration");
            }
        }
    }
}
