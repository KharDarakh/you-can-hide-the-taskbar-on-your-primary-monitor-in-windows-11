using System;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YCHTTOPYMIW11.App.ViewModels;
using YCHTTOPYMIW11.Core.Services;
using YCHTTOPYMIW11.Diagnostics.Services;

namespace YCHTTOPYMIW11.App
{
    public partial class App : System.Windows.Application
    {
        private ServiceProvider? _serviceProvider;
        private ITaskbarPolicyEngine? _policyEngine;
        private IConfigurationService? _configService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            _policyEngine = _serviceProvider.GetRequiredService<ITaskbarPolicyEngine>();
            _configService = _serviceProvider.GetRequiredService<IConfigurationService>();

            var config = _configService.LoadConfig();

            // Auto-apply saved policies on startup
            if (config.MonitorPolicies.Count > 0)
            {
                var displayManager = _serviceProvider.GetRequiredService<IDisplayManager>();
                var displays = displayManager.GetDisplays();

                foreach (var policyConfig in config.MonitorPolicies)
                {
                    var display = displays.FirstOrDefault(d => d.DeviceName.Equals(policyConfig.DeviceName, StringComparison.OrdinalIgnoreCase));
                    if (display != null)
                    {
                        _policyEngine.ApplyPolicy(display, policyConfig.Policy, policyConfig.CorrectDesktopWorkArea);
                    }
                }
            }

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

            bool startMinimized = e.Args.Contains("--minimized", StringComparer.OrdinalIgnoreCase) || config.StartMinimizedToTray;
            if (startMinimized)
            {
                mainWindow.WindowState = WindowState.Minimized;
                mainWindow.Hide();
            }
            else
            {
                mainWindow.Show();
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            services.AddSingleton<IDisplayManager, DisplayManager>();
            services.AddSingleton<ITaskbarDetector, TaskbarDetector>();
            services.AddSingleton<IWorkAreaManager, WorkAreaManager>();
            services.AddSingleton<ITaskbarPolicyEngine, TaskbarPolicyEngine>();
            services.AddSingleton<IShellRecoveryService, ShellRecoveryService>();
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IDiagnosticCollector, DiagnosticCollector>();
            services.AddSingleton<ISystemReportGenerator, SystemReportGenerator>();

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            var config = _configService?.LoadConfig();
            if (config?.RestoreOnExit ?? true)
            {
                _policyEngine?.RestoreAllPolicies();
            }

            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
