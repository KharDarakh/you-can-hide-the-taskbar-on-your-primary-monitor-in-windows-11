using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using YCHTTOPYMIW11.App.Services;
using YCHTTOPYMIW11.App.ViewModels;
using YCHTTOPYMIW11.Core.Models;

namespace YCHTTOPYMIW11.App
{
    public partial class MainWindow : Window
    {
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private readonly MainViewModel _viewModel;
        private NotifyIcon? _notifyIcon;
        private ToolStripMenuItem? _trayShowItem;
        private ToolStripMenuItem? _trayRestoreItem;
        private ToolStripMenuItem? _trayExitItem;
        private bool _isExplicitShutdown;
        private readonly List<(Border box, TextBlock text)> _accordionSegments = new();

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            _viewModel.RequestMinimizeToTray += () =>
            {
                WindowState = WindowState.Minimized;
                Hide();
            };

            _viewModel.RequestShutdown += () =>
            {
                _isExplicitShutdown = true;
                _notifyIcon?.Dispose();
                _notifyIcon = null;
                System.Windows.Application.Current.Shutdown();
            };

            InitializeNotifyIcon();
            SetupAccordionAnimation();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var helper = new WindowInteropHelper(this);
            if (helper.Handle != IntPtr.Zero)
            {
                ApplyTitleBarTheme(helper.Handle);
                var source = HwndSource.FromHwnd(helper.Handle);
                source?.AddHook(WndProc);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_SETTINGCHANGE = 0x001A;
            if (msg == WM_SETTINGCHANGE)
            {
                ApplyTitleBarTheme(hwnd);
            }
            return IntPtr.Zero;
        }

        private static void ApplyTitleBarTheme(IntPtr hwnd)
        {
            int useDarkMode = IsWindowsInDarkTheme() ? 1 : 0;
            if (DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int)) != 0)
            {
                DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref useDarkMode, sizeof(int));
            }
        }

        private static bool IsWindowsInDarkTheme()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var value = key?.GetValue("AppsUseLightTheme");
                if (value is int intVal)
                {
                    return intVal == 0;
                }
            }
            catch
            {
            }
            return true; // Default to dark theme
        }

        private void SetupAccordionAnimation()
        {
            _accordionSegments.Clear();
            _accordionSegments.Add((Box_ou, Txt_ou));
            _accordionSegments.Add((Box_an, Txt_an));
            _accordionSegments.Add((Box_ide, Txt_ide));
            _accordionSegments.Add((Box_he, Txt_he));
            _accordionSegments.Add((Box_askbar, Txt_askbar));
            _accordionSegments.Add((Box_n1, Txt_n1));
            _accordionSegments.Add((Box_our, Txt_our));
            _accordionSegments.Add((Box_rimary, Txt_rimary));
            _accordionSegments.Add((Box_onitor, Txt_onitor));
            _accordionSegments.Add((Box_n2, Txt_n2));
            _accordionSegments.Add((Box_indows, Txt_indows));

            AnimatedTitleContainer.MouseEnter += (s, e) => ExpandAccordion();
            AnimatedTitleContainer.MouseLeave += (s, e) => CollapseAccordion();
        }

        private void ExpandAccordion()
        {
            for (int i = 0; i < _accordionSegments.Count; i++)
            {
                var (box, text) = _accordionSegments[i];
                text.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
                double targetWidth = text.DesiredSize.Width;

                var anim = new DoubleAnimation
                {
                    To = targetWidth,
                    Duration = TimeSpan.FromMilliseconds(220),
                    BeginTime = TimeSpan.FromMilliseconds(i * 35),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                box.BeginAnimation(FrameworkElement.WidthProperty, anim);
            }
        }

        private void CollapseAccordion()
        {
            int count = _accordionSegments.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var (box, _) = _accordionSegments[i];
                int reverseIndex = count - 1 - i;

                var anim = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(180),
                    BeginTime = TimeSpan.FromMilliseconds(reverseIndex * 20),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };
                box.BeginAnimation(FrameworkElement.WidthProperty, anim);
            }
        }

        private void InitializeNotifyIcon()
        {
            System.Drawing.Icon? appIcon = null;

            try
            {
                var resourceUri = new Uri("pack://application:,,,/YCHTTOPYMIW11;component/Assets/app.ico", UriKind.RelativeOrAbsolute);
                var streamInfo = System.Windows.Application.GetResourceStream(resourceUri);
                if (streamInfo != null)
                {
                    using var ms = new System.IO.MemoryStream();
                    streamInfo.Stream.CopyTo(ms);
                    ms.Seek(0, System.IO.SeekOrigin.Begin);
                    appIcon = new System.Drawing.Icon(ms);
                }
            }
            catch
            {
                // Fallback to process executable icon
                try
                {
                    var exePath = Environment.ProcessPath ?? System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                    if (!string.IsNullOrEmpty(exePath) && System.IO.File.Exists(exePath))
                    {
                        appIcon = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
                    }
                }
                catch
                {
                    // Fallback to system icon
                }
            }

            var loc = LocalizationService.Instance;

            _notifyIcon = new NotifyIcon
            {
                Text = "YCHTTOPYMIW11",
                Icon = appIcon ?? System.Drawing.SystemIcons.Application,
                Visible = true
            };

            var contextMenu = new ContextMenuStrip();
            _trayShowItem = new ToolStripMenuItem(loc.TrayShow, null, (s, e) => RestoreFromTray());
            _trayRestoreItem = new ToolStripMenuItem(loc.TrayRestoreDefaults, null, (s, e) => _viewModel.RestoreDefaults());
            _trayExitItem = new ToolStripMenuItem(loc.TrayExit, null, (s, e) => CloseApplication());

            contextMenu.Items.Add(_trayShowItem);
            contextMenu.Items.Add(_trayRestoreItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(_trayExitItem);

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += (s, e) => RestoreFromTray();

            loc.LanguageChanged += UpdateTrayMenuTexts;
        }

        private void UpdateTrayMenuTexts()
        {
            var loc = LocalizationService.Instance;
            if (_trayShowItem != null) _trayShowItem.Text = loc.TrayShow;
            if (_trayRestoreItem != null) _trayRestoreItem.Text = loc.TrayRestoreDefaults;
            if (_trayExitItem != null) _trayExitItem.Text = loc.TrayExit;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            if (WindowState == WindowState.Minimized && _viewModel.StartMinimizedToTray)
            {
                Hide();
            }
        }

        private void RestoreFromTray()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void CloseApplication()
        {
            _isExplicitShutdown = true;
            _notifyIcon?.Dispose();
            _notifyIcon = null;
            System.Windows.Application.Current.Shutdown();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_isExplicitShutdown)
            {
                _notifyIcon?.Dispose();
                _notifyIcon = null;
                base.OnClosing(e);
                return;
            }

            if (_viewModel.CloseActionPreference == CloseAction.MinimizeToTray)
            {
                e.Cancel = true;
                WindowState = WindowState.Minimized;
                Hide();
                return;
            }

            if (_viewModel.CloseActionPreference == CloseAction.Exit)
            {
                _isExplicitShutdown = true;
                _notifyIcon?.Dispose();
                _notifyIcon = null;
                base.OnClosing(e);
                return;
            }

            // CloseAction.Ask -> Show modal overlay in UI
            e.Cancel = true;
            _viewModel.OpenCloseModal();
        }
    }
}
