using System;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using YCHTTOPYMIW11.Core.Models;
using YCHTTOPYMIW11.Core.Services;

namespace YCHTTOPYMIW11.Tests
{
    public class ConfigurationTests : IDisposable
    {
        private readonly string _testDir;

        public ConfigurationTests()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "YCHTTOPYMIW11Test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDir);
        }

        [Fact]
        public void AppConfig_HasSensibleDefaults()
        {
            var config = new AppConfig();
            Assert.Equal(1, config.Version);
            Assert.Equal(CloseAction.Ask, config.CloseAction);
            Assert.False(config.StartWithWindows);
            Assert.False(config.StartMinimizedToTray);
            Assert.True(config.EnableDiagnosticLogging);
            Assert.True(config.AutoRecoverOnExplorerRestart);
            Assert.True(config.AutoRecoverOnDisplayChange);
            Assert.True(config.RestoreOnExit);
            Assert.NotNull(config.MonitorPolicies);
            Assert.Empty(config.MonitorPolicies);
        }

        [Fact]
        public void MonitorPolicyConfig_CloneProducesExactIndependentCopy()
        {
            var original = new MonitorPolicyConfig
            {
                DeviceName = @"\\.\DISPLAY1",
                FriendlyName = "DISPLAY1 (Ana Ekran)",
                IsPrimary = true,
                Policy = TaskbarPolicy.PermanentlyHidden,
                CorrectDesktopWorkArea = true
            };

            var clone = original.Clone();

            Assert.Equal(original.DeviceName, clone.DeviceName);
            Assert.Equal(original.FriendlyName, clone.FriendlyName);
            Assert.Equal(original.IsPrimary, clone.IsPrimary);
            Assert.Equal(original.Policy, clone.Policy);
            Assert.Equal(original.CorrectDesktopWorkArea, clone.CorrectDesktopWorkArea);

            // Mutating clone should not mutate original
            clone.Policy = TaskbarPolicy.AlwaysVisible;
            Assert.Equal(TaskbarPolicy.PermanentlyHidden, original.Policy);
        }

        [Fact]
        public void ConfigurationService_SavesAndLoadsCorrectly()
        {
            var logger = NullLogger<ConfigurationService>.Instance;
            var service = new ConfigurationService(logger, _testDir);

            var config = service.LoadConfig();
            Assert.NotNull(config);

            config.StartWithWindows = true;
            config.CloseAction = CloseAction.MinimizeToTray;
            config.MonitorPolicies.Add(new MonitorPolicyConfig
            {
                DeviceName = @"\\.\DISPLAY1",
                Policy = TaskbarPolicy.PermanentlyHidden,
                CorrectDesktopWorkArea = true
            });

            service.SaveConfig(config);

            var reloaded = service.LoadConfig();
            Assert.True(reloaded.StartWithWindows);
            Assert.Equal(CloseAction.MinimizeToTray, reloaded.CloseAction);
            Assert.Single(reloaded.MonitorPolicies);
            Assert.Equal(@"\\.\DISPLAY1", reloaded.MonitorPolicies[0].DeviceName);
            Assert.Equal(TaskbarPolicy.PermanentlyHidden, reloaded.MonitorPolicies[0].Policy);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_testDir))
                {
                    Directory.Delete(_testDir, true);
                }
            }
            catch
            {
            }
        }
    }
}
