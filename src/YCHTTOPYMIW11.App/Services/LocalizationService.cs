using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace YCHTTOPYMIW11.App.Services
{
    public sealed class LocalizationService : INotifyPropertyChanged
    {
        public static LocalizationService Instance { get; } = new LocalizationService();

        private string _currentLanguage = "en";

        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action? LanguageChanged;

        private LocalizationService()
        {
            _currentLanguage = DetectSystemLanguage();
        }

        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                string normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
                if (normalized != "tr" && normalized != "en")
                {
                    normalized = "en";
                }

                if (_currentLanguage != normalized)
                {
                    _currentLanguage = normalized;
                    OnPropertyChanged(string.Empty); // Notify all properties changed
                    LanguageChanged?.Invoke();
                }
            }
        }

        public bool IsTurkish => _currentLanguage == "tr";
        public bool IsEnglish => _currentLanguage == "en";

        public static string DetectSystemLanguage()
        {
            try
            {
                string uiLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
                if (uiLang == "tr" || uiLang == "az")
                    return "tr";
            }
            catch { }

            try
            {
                string instLang = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
                if (instLang == "tr" || instLang == "az")
                    return "tr";
            }
            catch { }

            return "en";
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // --- Localized Strings ---

        // Header & General
        public string Subtitle => IsTurkish
            ? "Windows 11 Çoklu Monitör Görev Çubuğu Yöneticisi"
            : "Windows 11 Multi-Monitor Taskbar Manager";

        public string Refresh => IsTurkish ? "Yenile" : "Refresh";
        public string StatusLabel => IsTurkish ? "Durum:" : "Status:";
        public string Ready => IsTurkish ? "Hazır" : "Ready";
        public string ApplyingPolicies => IsTurkish ? "İlkeler uygulanıyor..." : "Applying policies...";
        public string AppliedSuccess => IsTurkish
            ? "Tüm ekranlar için görev çubuğu ilkeleri başarıyla uygulandı."
            : "Taskbar policies applied successfully for all displays.";
        public string ApplyError => IsTurkish
            ? "İlkeler uygulanırken hata oluştu: {0}"
            : "Error applying policies: {0}";
        public string RestoringDefaults => IsTurkish
            ? "Varsayılan ayarlar geri yükleniyor..."
            : "Restoring default settings...";
        public string RestoredSuccess => IsTurkish
            ? "Tüm görev çubukları ve çalışma alanları başarıyla geri yüklendi."
            : "All taskbars and work areas restored successfully.";
        public string RestoreError => IsTurkish
            ? "Geri yükleme sırasında hata oluştu: {0}"
            : "Error during restore: {0}";
        public string GeneratingReport => IsTurkish ? "Sistem raporu oluşturuluyor..." : "Generating system report...";
        public string ReportGenerated => IsTurkish
            ? "Sistem tanılama raporu başarıyla oluşturuldu."
            : "System diagnostic report generated successfully.";

        // Tabs
        public string TabTaskbars => IsTurkish ? "Görev Çubukları" : "Taskbars";
        public string TabSettings => IsTurkish ? "Uygulama ve Kurtarma Ayarları" : "App & Recovery Settings";

        // Taskbars Tab
        public string RestoreDefaults => IsTurkish ? "Varsayılanları Geri Yükle" : "Restore Defaults";
        public string ApplyPolicies => IsTurkish ? "İlkeleri Uygula" : "Apply Policies";
        public string PrimaryBadge => IsTurkish ? "ANA EKRAN" : "PRIMARY DISPLAY";
        public string TaskbarLabel => IsTurkish ? "Görev Çubuğu: " : "Taskbar: ";
        public string TaskbarPolicyLabel => IsTurkish ? "Görev Çubuğu İlkesi" : "Taskbar Policy";
        public string PolicyAlwaysVisible => IsTurkish ? "Her Zaman Görünür" : "Always Visible";
        public string PolicyPermanentlyHidden => IsTurkish ? "Kalıcı Olarak Gizli" : "Permanently Hidden";
        public string StateVisible => IsTurkish ? "Görünür" : "Visible";
        public string StateHidden => IsTurkish ? "Kalıcı Olarak Gizli" : "Permanently Hidden";
        public string StateNotFound => IsTurkish ? "Görev çubuğu bulunamadı" : "Taskbar not found";
        public string ReservedAreaNone => IsTurkish ? "Ayrılmış alan yok (Tam Ekran)" : "No reserved area (Full Screen)";
        public string ReservedAreaFormatted => IsTurkish ? "{0} px (Ayrılmış Alan)" : "{0} px (Reserved Area)";

        // Settings Tab
        public string SystemStartupHeader => IsTurkish ? "Sistem Başlangıcı" : "System Startup";
        public string StartWithWindows => IsTurkish
            ? "Windows açılışında otomatik başlat"
            : "Start automatically with Windows";
        public string StartMinimizedToTray => IsTurkish
            ? "Sistem bildirim alanında (tepside) simge durumunda başlat"
            : "Start minimized to system tray";
        public string RecoverySecurityHeader => IsTurkish ? "Kurtarma ve Güvenlik" : "Recovery & Resilience";
        public string AutoRecoverExplorer => IsTurkish
            ? "Windows Gezgini (Explorer) yeniden başladığında görev çubuğu ilkelerini otomatik olarak yeniden uygula"
            : "Automatically re-apply taskbar policies when Windows Explorer restarts";
        public string AutoRecoverDisplay => IsTurkish
            ? "Monitör bağlandığında/çıkarıldığında veya çözünürlük değiştiğinde ilkeleri otomatik olarak uyarla"
            : "Automatically adapt policies when displays change (connect/disconnect/resolution)";
        public string DiagnosticReportHeader => IsTurkish ? "Tanılama ve Raporlama" : "Diagnostics & Reporting";
        public string EnableDiagnosticLogging => IsTurkish
            ? "Ayrıntılı tanılama günlük kaydını (Diagnostic Logging) etkinleştir"
            : "Enable detailed diagnostic logging";
        public string DetailedSystemReport => IsTurkish ? "Detaylı Sistem Raporu" : "Detailed System Report";

        // Close Action Settings
        public string CloseActionHeader => IsTurkish ? "Pencere Kapatma Davranışı" : "Window Close Behavior";
        public string CloseActionDescription => IsTurkish
            ? "Pencerenin sağ üstündeki kapatma (X) tuşuna basıldığında:"
            : "When clicking the close (X) button on the window:";
        public string CloseActionAsk => IsTurkish
            ? "Her zaman sor (Onay penceresi göster)"
            : "Always ask (Show confirmation dialog)";
        public string CloseActionMinimize => IsTurkish
            ? "Sistem tepsisine küçült (Arka planda çalış)"
            : "Minimize to system tray (Keep running in background)";
        public string CloseActionExit => IsTurkish
            ? "Uygulamayı tamamen kapat"
            : "Completely exit application";
        public string ResetCloseAction => IsTurkish
            ? "Kapatma Tercihini Sıfırla (Tekrar Sor)"
            : "Reset Close Preference (Ask Again)";
        public string CloseActionResetSuccess => IsTurkish
            ? "Kapatma tercihi sıfırlandı; kapatırken onay penceresi tekrar sorulacak."
            : "Close preference reset; confirmation dialog will be shown on close.";

        // Close Confirmation Modal
        public string CloseModalTitle => IsTurkish ? "Uygulamadan Çıkılsın mı?" : "Exit Application?";
        public string CloseModalDescription => IsTurkish
            ? "Pencereyi kapattığınızda uygulamanın nasıl davranmasını istersiniz?"
            : "What would you like the application to do when closing the window?";
        public string CloseModalRememberChoice => IsTurkish
            ? "Bu seçimimi hatırla, bir daha sorma"
            : "Remember my choice, don't ask again";
        public string CloseModalMinimize => IsTurkish
            ? "Sistem Tepsisine Küçült"
            : "Minimize to Tray";
        public string CloseModalExit => IsTurkish
            ? "Uygulamayı Kapat"
            : "Exit Application";

        // Diagnostic Modal
        public string DiagnosticModalTitle => IsTurkish ? "Sistem Tanılama Raporu" : "System Diagnostic Report";
        public string CloseModal => IsTurkish ? "Kapat" : "Close";

        // Tray Context Menu
        public string TrayShow => IsTurkish ? "Göster" : "Show";
        public string TrayRestoreDefaults => IsTurkish ? "Varsayılanları Geri Yükle" : "Restore Defaults";
        public string TrayExit => IsTurkish ? "Çıkış" : "Exit";
    }
}

