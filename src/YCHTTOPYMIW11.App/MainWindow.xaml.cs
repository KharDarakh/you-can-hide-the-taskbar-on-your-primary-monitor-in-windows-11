using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using YCHTTOPYMIW11.App.ViewModels;

namespace YCHTTOPYMIW11.App
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private NotifyIcon? _notifyIcon;
        private readonly List<(Border box, TextBlock text)> _accordionSegments = new();

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            InitializeNotifyIcon();
            SetupAccordionAnimation();
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

            _notifyIcon = new NotifyIcon
            {
                Text = "YCHTTOPYMIW11",
                Icon = appIcon ?? System.Drawing.SystemIcons.Application,
                Visible = true
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Göster", null, (s, e) => RestoreFromTray());
            contextMenu.Items.Add("Varsayılanları Geri Yükle", null, (s, e) => _viewModel.RestoreDefaults());
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Çıkış", null, (s, e) => CloseApplication());

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += (s, e) => RestoreFromTray();
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
            _notifyIcon?.Dispose();
            _notifyIcon = null;
            System.Windows.Application.Current.Shutdown();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _notifyIcon?.Dispose();
            _notifyIcon = null;
            base.OnClosing(e);
        }
    }
}
