using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using YCHTTOPYMIW11.App.Services;
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

        private string _statusText = string.Empty;
        private bool _isStatusError;
        private bool _isBusy;
        private string _diagnosticReportText = string.Empty;
        private bool _isReportModalOpen;

        // Close Action State
        private CloseAction _closeAction = CloseAction.Ask;
        private bool _isCloseModalOpen;
        private bool _closeModalRememberChoice;

        // Settings
        private bool _startWithWindows;
        private bool _startMinimizedToTray;
        private bool _enableDiagnosticLogging;
        private bool _autoRecoverOnExplorerRestart;
        private bool _autoRecoverOnDisplayChange;

        public LocalizationService Loc => LocalizationService.Instance;

        public ObservableCollection<MonitorCardViewModel> Monitors { get; } = new();

        public ICommand ApplyCommand { get; }
        public ICommand RestoreDefaultsCommand { get; }
        public ICommand GenerateReportCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CloseReportModalCommand { get; }

        public ICommand ConfirmMinimizeToTrayCommand { get; }
        public ICommand ConfirmExitCommand { get; }
        public ICommand CancelCloseModalCommand { get; }
        public ICommand ResetCloseActionCommand { get; }

        public event Action? RequestMinimizeToTray;
        public event Action? RequestShutdown;

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

            ConfirmMinimizeToTrayCommand = new RelayCommand(OnConfirmMinimizeToTray);
            ConfirmExitCommand = new RelayCommand(OnConfirmExit);
            CancelCloseModalCommand = new RelayCommand(() => IsCloseModalOpen = false);
            ResetCloseActionCommand = new RelayCommand(ResetCloseAction);

            _recoveryService.ExplorerRestarted += OnExplorerRestarted;
            _recoveryService.DisplayTopologyChanged += OnDisplayTopologyChanged;
            _recoveryService.StartListening();

            LocalizationService.Instance.LanguageChanged += OnLanguageChanged;

            LoadConfiguration();
            _statusText = Loc.Ready;
            RefreshState();
        }

        private void OnLanguageChanged()
        {
            OnPropertyChanged(nameof(SelectedLanguageIndex));
            OnPropertyChanged(nameof(SelectedCloseActionIndex));
            OnPropertyChanged(nameof(Loc));
            SetStatus(Loc.Ready, false);
        }

        public int SelectedLanguageIndex
        {
            get => Loc.IsTurkish ? 0 : 1;
            set
            {
                string target = value == 0 ? "tr" : "en";
                if (Loc.CurrentLanguage != target)
                {
                    Loc.CurrentLanguage = target;
                    SaveConfiguration();
                }
            }
        }

        public CloseAction CloseActionPreference
        {
            get => _closeAction;
            set
            {
                if (SetField(ref _closeAction, value))
                {
                    OnPropertyChanged(nameof(SelectedCloseActionIndex));
                    SaveConfiguration();
                }
            }
        }

        public int SelectedCloseActionIndex
        {
            get => (int)_closeAction;
            set
            {
                if (value >= 0 && value <= 2)
                {
                    CloseActionPreference = (CloseAction)value;
                }
            }
        }

        public bool IsCloseModalOpen
        {
            get => _isCloseModalOpen;
            set => SetField(ref _isCloseModalOpen, value);
        }

        public bool CloseModalRememberChoice
        {
            get => _closeModalRememberChoice;
            set => SetField(ref _closeModalRememberChoice, value);
        }

        public void OpenCloseModal()
        {
            CloseModalRememberChoice = false;
            IsCloseModalOpen = true;
        }

        private void OnConfirmMinimizeToTray()
        {
            if (CloseModalRememberChoice)
            {
                CloseActionPreference = CloseAction.MinimizeToTray;
            }
            IsCloseModalOpen = false;
            RequestMinimizeToTray?.Invoke();
        }

        private void OnConfirmExit()
        {
            if (CloseModalRememberChoice)
            {
                CloseActionPreference = CloseAction.Exit;
            }
            IsCloseModalOpen = false;
            RequestShutdown?.Invoke();
        }

        public void ResetCloseAction()
        {
            CloseActionPreference = CloseAction.Ask;
            SetStatus(Loc.CloseActionResetSuccess, false);
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

                SetStatus(Loc.Ready, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh state");
                SetStatus(string.Format(Loc.ApplyError, ex.Message), true);
            }
        }

        public async Task ApplyPoliciesAsync()
        {
            IsBusy = true;
            SetStatus(Loc.ApplyingPolicies, false);

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
                    SetStatus(Loc.AppliedSuccess, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error applying policies");
                    SetStatus(string.Format(Loc.ApplyError, ex.Message), true);
                }
            });

            RefreshState();
            IsBusy = false;
        }

        public void RestoreDefaults()
        {
            try
            {
                SetStatus(Loc.RestoringDefaults, false);
                _policyEngine.RestoreAllPolicies();
                _configService.ResetToDefaults();
                RefreshState();
                SetStatus(Loc.RestoredSuccess, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed restoring defaults");
                SetStatus(string.Format(Loc.RestoreError, ex.Message), true);
            }
        }

        public void GenerateReport()
        {
            try
            {
                SetStatus(Loc.GeneratingReport, false);
                var snapshot = _diagnosticCollector.CaptureSnapshot();
                DiagnosticReportText = _reportGenerator.GenerateSnapshotReport(snapshot);
                IsReportModalOpen = true;
                SetStatus(Loc.ReportGenerated, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate report");
                SetStatus(string.Format(Loc.ApplyError, ex.Message), true);
            }
        }

        private void LoadConfiguration()
        {
            var config = _configService.LoadConfig();

            if (!string.IsNullOrWhiteSpace(config.Language))
            {
                Loc.CurrentLanguage = config.Language;
            }
            else
            {
                Loc.CurrentLanguage = LocalizationService.DetectSystemLanguage();
            }

            _closeAction = config.CloseAction;
            _startWithWindows = config.StartWithWindows;
            _startMinimizedToTray = config.StartMinimizedToTray;
            _enableDiagnosticLogging = config.EnableDiagnosticLogging;
            _autoRecoverOnExplorerRestart = config.AutoRecoverOnExplorerRestart;
            _autoRecoverOnDisplayChange = config.AutoRecoverOnDisplayChange;

            OnPropertyChanged(nameof(SelectedLanguageIndex));
            OnPropertyChanged(nameof(SelectedCloseActionIndex));
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
                Language = Loc.CurrentLanguage,
                CloseAction = CloseActionPreference,
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
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed updating startup registry");
            }
        }

        private void OnExplorerRestarted(object? sender, EventArgs e)
        {
            _logger.LogInformation("Explorer restart detected. Re-applying policies...");
            App.Current?.Dispatcher?.Invoke(async () =>
            {
                if (AutoRecoverOnExplorerRestart)
                {
                    await ApplyPoliciesAsync();
                }
                else
                {
                    RefreshState();
                }
            });
        }

        private void OnDisplayTopologyChanged(object? sender, EventArgs e)
        {
            _logger.LogInformation("Display topology change detected. Refreshing...");
            App.Current?.Dispatcher?.Invoke(async () =>
            {
                if (AutoRecoverOnDisplayChange)
                {
                    await ApplyPoliciesAsync();
                }
                else
                {
                    RefreshState();
                }
            });
        }

        private void SetStatus(string message, bool isError)
        {
            StatusText = message;
            IsStatusError = isError;
        }
    }
}
