using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using YCHTTOPYMIW11.Core.Models;
using YCHTTOPYMIW11.Core.Services;
using YCHTTOPYMIW11.Diagnostics.Models;
using YCHTTOPYMIW11.Diagnostics.Services;

namespace YCHTTOPYMIW11.App.ViewModels
{
    public sealed class MainViewModel : ViewModelBase
    {
        private readonly ILogger<MainViewModel> _logger;
        private readonly IDisplayManager _displayManager;
        private readonly ITaskbarDetector _taskbarDetector;
        private readonly IWorkAreaManager _workAreaManager;
        private readonly ITaskbarPolicyEngine _policyEngine;
        private readonly IShellRecoveryService _recoveryService;
        private readonly IConfigurationService _configService;
        private readonly IDiagnosticCollector _diagnosticCollector;
        private readonly ISystemReportGenerator _reportGenerator;

        private string _statusText = "Hazır";
        private bool _isStatusError;
        private bool _isBusy;
        private string _diagnosticReportText = string.Empty;
        private bool _isReportModalOpen;

        // Settings
        private bool _startWithWindows;
        private bool _startMinimizedToTray;
        private bool _enableDiagnosticLogging;
        private bool _autoRecoverOnExplorerRestart;
        private bool _autoRecoverOnDisplayChange;

        public ObservableCollection<MonitorCardViewModel> Monitors { get; } = new();

        public ICommand ApplyCommand { get; }
        public ICommand RestoreDefaultsCommand { get; }
        public ICommand GenerateReportCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CloseReportModalCommand { get; }

        public MainViewModel(
            ILogger<MainViewModel> logger,
            IDisplayManager displayManager,
            ITaskbarDetector taskbarDetector,
            IWorkAreaManager workAreaManager,
            ITaskbarPolicyEngine policyEngine,
            IShellRecoveryService recoveryService,
            IConfigurationService configService,
            IDiagnosticCollector diagnosticCollector,
            ISystemReportGenerator reportGenerator)
        {
            _logger = logger;
            _displayManager = displayManager;
            _taskbarDetector = taskbarDetector;
            _workAreaManager = workAreaManager;
            _policyEngine = policyEngine;
            _recoveryService = recoveryService;
            _configService = configService;
            _diagnosticCollector = diagnosticCollector;
            _reportGenerator = reportGenerator;

            ApplyCommand = new RelayCommand(async () => await ApplyPoliciesAsync(), () => !IsBusy);
            RestoreDefaultsCommand = new RelayCommand(RestoreDefaults, () => !IsBusy);
            GenerateReportCommand = new RelayCommand(GenerateReport, () => !IsBusy);
            RefreshCommand = new RelayCommand(RefreshState, () => !IsBusy);
            CloseReportModalCommand = new RelayCommand(() => IsReportModalOpen = false);

            _recoveryService.ExplorerRestarted += OnExplorerRestarted;
            _recoveryService.DisplayTopologyChanged += OnDisplayTopologyChanged;
            _recoveryService.StartListening();

            LoadConfiguration();
            RefreshState();
        }

        public string StatusText
        {
            get => _statusText;
            set => SetField(ref _statusText, value);
        }

        public bool IsStatusError
        {
            get => _isStatusError;
            set => SetField(ref _isStatusError, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetField(ref _isBusy, value))
                {
                    (ApplyCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (RestoreDefaultsCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (GenerateReportCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (RefreshCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string DiagnosticReportText
        {
            get => _diagnosticReportText;
            set => SetField(ref _diagnosticReportText, value);
        }

        public bool IsReportModalOpen
        {
            get => _isReportModalOpen;
            set => SetField(ref _isReportModalOpen, value);
        }

        public bool StartWithWindows
        {
            get => _startWithWindows;
            set
            {
                if (SetField(ref _startWithWindows, value))
                {
                    UpdateStartupRegistry(value);
                    SaveConfiguration();
                }
            }
        }

        public bool StartMinimizedToTray
        {
            get => _startMinimizedToTray;
            set
            {
                if (SetField(ref _startMinimizedToTray, value))
                {
                    SaveConfiguration();
                }
            }
        }

        public bool EnableDiagnosticLogging
        {
            get => _enableDiagnosticLogging;
            set
            {
                if (SetField(ref _enableDiagnosticLogging, value))
                {
                    SaveConfiguration();
                }
            }
        }

        public bool AutoRecoverOnExplorerRestart
        {
            get => _autoRecoverOnExplorerRestart;
            set
            {
                if (SetField(ref _autoRecoverOnExplorerRestart, value))
                {
                    SaveConfiguration();
                }
            }
        }

        public bool AutoRecoverOnDisplayChange
        {
            get => _autoRecoverOnDisplayChange;
            set
            {
                if (SetField(ref _autoRecoverOnDisplayChange, value))
                {
                    SaveConfiguration();
                }
            }
        }

        public void RefreshState()
        {
            try
            {
                var displays = _displayManager.GetDisplays();
                var taskbars = _taskbarDetector.DetectTaskbars();

                Monitors.Clear();
                foreach (var display in displays)
                {
                    var card = new MonitorCardViewModel(display);
                    var taskbar = taskbars.FirstOrDefault(t => t.MonitorHandle == display.MonitorHandle);
                    card.UpdateTaskbarState(taskbar);

                    var currentPolicy = _policyEngine.GetCurrentPolicy(display);
                    card.SetPolicy(currentPolicy);

                    Monitors.Add(card);
                }

                SetStatus("Sistem durumu başarıyla güncellendi.", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh state");
                SetStatus("Ekran bilgileri alınamadı: " + ex.Message, true);
            }
        }

        public async Task ApplyPoliciesAsync()
        {
            IsBusy = true;
            SetStatus("İlkeler uygulanıyor...", false);

            await Task.Run(() =>
            {
                try
                {
                    var policyList = Monitors.Select(c => (c.DisplayInfo, c.SelectedPolicy, c.CorrectWorkArea)).ToList();
                    bool ok = _policyEngine.ApplyAllPolicies(policyList);

                    if (!ok)
                    {
                        _logger.LogWarning("Some policies could not be applied completely.");
                    }

                    SaveConfiguration();
                    SetStatus("Görev çubuğu ilkeleri ve masaüstü alanları başarıyla uygulandı.", false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error applying policies");
                    SetStatus("İlke uygulama hatası: " + ex.Message, true);
                }
            });

            RefreshState();
            IsBusy = false;
        }

        public void RestoreDefaults()
        {
            try
            {
                _policyEngine.RestoreAllPolicies();
                _configService.ResetToDefaults();
                RefreshState();
                SetStatus("Tüm ayarlar ve görev çubukları varsayılan Windows durumuna döndürüldü.", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed restoring defaults");
                SetStatus("Sıfırlama hatası: " + ex.Message, true);
            }
        }

        public void GenerateReport()
        {
            try
            {
                var snapshot = _diagnosticCollector.CaptureSnapshot();
                DiagnosticReportText = _reportGenerator.GenerateSnapshotReport(snapshot);
                IsReportModalOpen = true;
                SetStatus("Sistem tanılama raporu oluşturuldu.", false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate report");
                SetStatus("Rapor oluşturma hatası: " + ex.Message, true);
            }
        }

        private void LoadConfiguration()
        {
            var config = _configService.LoadConfig();
            _startWithWindows = config.StartWithWindows;
            _startMinimizedToTray = config.StartMinimizedToTray;
            _enableDiagnosticLogging = config.EnableDiagnosticLogging;
            _autoRecoverOnExplorerRestart = config.AutoRecoverOnExplorerRestart;
            _autoRecoverOnDisplayChange = config.AutoRecoverOnDisplayChange;

            OnPropertyChanged(nameof(StartWithWindows));
            OnPropertyChanged(nameof(StartMinimizedToTray));
            OnPropertyChanged(nameof(EnableDiagnosticLogging));
            OnPropertyChanged(nameof(AutoRecoverOnExplorerRestart));
            OnPropertyChanged(nameof(AutoRecoverOnDisplayChange));
        }

        private void SaveConfiguration()
        {
            var config = new AppConfig
            {
                StartWithWindows = StartWithWindows,
                StartMinimizedToTray = StartMinimizedToTray,
                EnableDiagnosticLogging = EnableDiagnosticLogging,
                AutoRecoverOnExplorerRestart = AutoRecoverOnExplorerRestart,
                AutoRecoverOnDisplayChange = AutoRecoverOnDisplayChange,
                MonitorPolicies = Monitors.Select(m => new MonitorPolicyConfig
                {
                    DeviceName = m.DeviceName,
                    FriendlyName = m.FriendlyName,
                    IsPrimary = m.IsPrimary,
                    Policy = m.SelectedPolicy,
                    CorrectDesktopWorkArea = m.CorrectWorkArea
                }).ToList()
            };

            _configService.SaveConfig(config);
        }

        private void UpdateStartupRegistry(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                if (key != null)
                {
                    string appPath = Environment.ProcessPath ?? string.Empty;
                    if (enable && !string.IsNullOrEmpty(appPath))
                    {
                        key.SetValue("YCHTTOPYMIW11", $"\"{appPath}\" --minimized");
                    }
                    else
                    {
                        key.DeleteValue("YCHTTOPYMIW11", false);
                        key.DeleteValue("YCHTTOPYMIW11", false);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not update Windows startup registry key");
            }
        }

        private void OnExplorerRestarted(object? sender, EventArgs e)
        {
            if (!AutoRecoverOnExplorerRestart) return;

            _logger.LogInformation("Automatic recovery triggered: Explorer restarted");
            App.Current?.Dispatcher.InvokeAsync(async () =>
            {
                SetStatus("Windows Gezgini yeniden başlatıldı. İlkeler otomatik olarak geri yükleniyor...", false);
                await ApplyPoliciesAsync();
            });
        }

        private void OnDisplayTopologyChanged(object? sender, EventArgs e)
        {
            if (!AutoRecoverOnDisplayChange) return;

            _logger.LogInformation("Automatic recovery triggered: Display topology changed");
            App.Current?.Dispatcher.InvokeAsync(async () =>
            {
                SetStatus("Ekran düzeni değişti. Algılanıyor...", false);
                RefreshState();
                await ApplyPoliciesAsync();
            });
        }

        private void SetStatus(string message, bool isError)
        {
            StatusText = message;
            IsStatusError = isError;
        }
    }
}

