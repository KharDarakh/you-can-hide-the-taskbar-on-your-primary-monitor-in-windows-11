using System;
using YCHTTOPYMIW11.App.Services;
using YCHTTOPYMIW11.Core.Models;

namespace YCHTTOPYMIW11.App.ViewModels
{
    public sealed class MonitorCardViewModel : ViewModelBase
    {
        private string _friendlyName = string.Empty;
        private string _deviceName = string.Empty;
        private bool _isPrimary;
        private string _resolutionText = string.Empty;
        private string _boundsText = string.Empty;
        private string _workAreaText = string.Empty;
        private string _reservedAreaText = string.Empty;
        private string _dpiText = string.Empty;
        private string _taskbarStateText = string.Empty;
        private bool _isTaskbarVisible = true;
        private int _selectedPolicyIndex = 0; // 0: AlwaysVisible, 1: PermanentlyHidden
        private string _statusMessage = string.Empty;
        private TaskbarWindowInfo? _lastTaskbar;

        public DisplayInfo DisplayInfo { get; }
        public LocalizationService Loc => LocalizationService.Instance;

        public MonitorCardViewModel(DisplayInfo displayInfo)
        {
            DisplayInfo = displayInfo;
            LocalizationService.Instance.LanguageChanged += OnLanguageChanged;
            UpdateFromDisplay(displayInfo);
        }

        private void OnLanguageChanged()
        {
            RefreshTexts();
        }

        public string FriendlyName
        {
            get => _friendlyName;
            set => SetField(ref _friendlyName, value);
        }

        public string DeviceName
        {
            get => _deviceName;
            set => SetField(ref _deviceName, value);
        }

        public bool IsPrimary
        {
            get => _isPrimary;
            set => SetField(ref _isPrimary, value);
        }

        public string ResolutionText
        {
            get => _resolutionText;
            set => SetField(ref _resolutionText, value);
        }

        public string BoundsText
        {
            get => _boundsText;
            set => SetField(ref _boundsText, value);
        }

        public string WorkAreaText
        {
            get => _workAreaText;
            set => SetField(ref _workAreaText, value);
        }

        public string ReservedAreaText
        {
            get => _reservedAreaText;
            set => SetField(ref _reservedAreaText, value);
        }

        public string DpiText
        {
            get => _dpiText;
            set => SetField(ref _dpiText, value);
        }

        public string TaskbarStateText
        {
            get => _taskbarStateText;
            set => SetField(ref _taskbarStateText, value);
        }

        public bool IsTaskbarVisible
        {
            get => _isTaskbarVisible;
            set => SetField(ref _isTaskbarVisible, value);
        }

        public int SelectedPolicyIndex
        {
            get => _selectedPolicyIndex;
            set => SetField(ref _selectedPolicyIndex, value);
        }

        public bool CorrectWorkArea => SelectedPolicy == TaskbarPolicy.PermanentlyHidden;

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetField(ref _statusMessage, value);
        }

        public TaskbarPolicy SelectedPolicy => SelectedPolicyIndex switch
        {
            1 => TaskbarPolicy.PermanentlyHidden,
            _ => TaskbarPolicy.AlwaysVisible
        };

        public void SetPolicy(TaskbarPolicy policy)
        {
            SelectedPolicyIndex = policy switch
            {
                TaskbarPolicy.PermanentlyHidden => 1,
                _ => 0
            };
        }

        public string SummaryText => $"{ResolutionText} • {DpiText}";
        public string TaskbarStateColor => IsTaskbarVisible ? "#4CAF50" : "#FFA726";

        public void UpdateFromDisplay(DisplayInfo display)
        {
            FriendlyName = display.FriendlyName;
            DeviceName = display.DeviceName;
            IsPrimary = display.IsPrimary;
            ResolutionText = $"{display.Width} × {display.Height}";
            BoundsText = $"({display.MonitorBounds.Left}, {display.MonitorBounds.Top}, {display.MonitorBounds.Right}, {display.MonitorBounds.Bottom})";
            WorkAreaText = $"({display.WorkArea.Left}, {display.WorkArea.Top}, {display.WorkArea.Right}, {display.WorkArea.Bottom}) [{display.WorkArea.Width}x{display.WorkArea.Height}]";

            RefreshTexts();
        }

        public void UpdateTaskbarState(TaskbarWindowInfo? taskbar)
        {
            _lastTaskbar = taskbar;
            RefreshTexts();
        }

        public void RefreshTexts()
        {
            var loc = LocalizationService.Instance;
            int reserved = DisplayInfo.Height - DisplayInfo.WorkArea.Height;
            ReservedAreaText = reserved > 0
                ? string.Format(loc.ReservedAreaFormatted, reserved)
                : loc.ReservedAreaNone;

            DpiText = $"{DisplayInfo.DpiX} DPI (%{DisplayInfo.ScaleFactor * 100:0})";

            if (_lastTaskbar == null)
            {
                TaskbarStateText = loc.StateNotFound;
                IsTaskbarVisible = false;
            }
            else
            {
                IsTaskbarVisible = _lastTaskbar.IsVisible;
                TaskbarStateText = _lastTaskbar.IsVisible ? loc.StateVisible : loc.StateHidden;
            }

            OnPropertyChanged(nameof(SummaryText));
            OnPropertyChanged(nameof(ReservedAreaText));
            OnPropertyChanged(nameof(DpiText));
            OnPropertyChanged(nameof(TaskbarStateText));
            OnPropertyChanged(nameof(TaskbarStateColor));
        }
    }
}
