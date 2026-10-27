using System;
using System.Text;
using YCHTTOPYMIW11.Diagnostics.Models;

namespace YCHTTOPYMIW11.Diagnostics.Services
{
    public sealed class SystemReportGenerator : ISystemReportGenerator
    {
        public string GenerateSnapshotReport(DiagnosticSnapshot s)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# YCHTTOPYMIW11 Windows 11 Sistem Tanılama Raporu");
            sb.AppendLine($"Rapor Oluşturulma Zamanı (UTC): {s.Timestamp:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            sb.AppendLine("## 1. Algılanan Monitörler ve Çalışma Alanları");
            foreach (var d in s.Displays)
            {
                sb.AppendLine($"- **{d.FriendlyName}** ({d.DeviceName})");
                sb.AppendLine($"  - Ana Ekran: {(d.IsPrimary ? "Evet" : "Hayır")}");
                sb.AppendLine($"  - Tam Ekran Çözünürlüğü: {d.MonitorBounds} ({d.Width}x{d.Height})");
                sb.AppendLine($"  - Bildirilen Çalışma Alanı: {d.WorkArea} ({d.WorkArea.Width}x{d.WorkArea.Height})");
                sb.AppendLine($"  - Görev Çubuğu Ayrılmış Alanı: {d.Height - d.WorkArea.Height} px");
                sb.AppendLine($"  - DPI: {d.DpiX}x{d.DpiY} (Ölçek: %{d.ScaleFactor * 100:0})");
            }
            sb.AppendLine();

            sb.AppendLine("## 2. Görev Çubuğu Pencereleri");
            if (s.Taskbars.Count == 0)
            {
                sb.AppendLine("- Görev çubuğu penceresi algılanamadı.");
            }
            else
            {
                foreach (var t in s.Taskbars)
                {
                    sb.AppendLine($"- **{t.ClassName}** (Rol: {t.Role}, HWND: 0x{t.Handle:X})");
                    sb.AppendLine($"  - Görünürlük Durumu: {(t.IsVisible ? "Görünür (Visible)" : "Gizli (Hidden)")}");
                    sb.AppendLine($"  - Konum ve Boyut: {t.Bounds} ({t.Bounds.Width}x{t.Bounds.Height})");
                    sb.AppendLine($"  - Ait Olduğu Ekran: {t.MonitorDeviceName} (HMON: 0x{t.MonitorHandle:X})");
                    sb.AppendLine($"  - Pencere Stilleri: Style=0x{t.Style:X8}, ExStyle=0x{t.ExtendedStyle:X8}");
                }
            }
            sb.AppendLine();

            sb.AppendLine("## 3. Masaüstü Simge Görünümü (Shell View Hierarchy)");
            var dv = s.DesktopView;
            sb.AppendLine($"- **Progman HWND:** 0x{dv.ProgmanHandle:X}");
            if (dv.WorkerWHandle != IntPtr.Zero)
            {
                sb.AppendLine($"- **WorkerW HWND:** 0x{dv.WorkerWHandle:X}");
            }
            sb.AppendLine($"- **SHELLDLL_DefView HWND:** 0x{dv.DefViewHandle:X}");
            sb.AppendLine($"  - Sınırlar: {dv.DefViewBounds}, İstemci Alanı: {dv.DefViewClientRect}");
            sb.AppendLine($"- **SysListView32 HWND:** 0x{dv.ListViewHandle:X}");
            sb.AppendLine($"  - Sınırlar: {dv.ListViewBounds}, İstemci Alanı: {dv.ListViewClientRect}");
            sb.AppendLine($"  - Stil: 0x{dv.ListViewStyle:X8}");
            sb.AppendLine($"  - Toplam Masaüstü Simgesi Sayısı: {dv.IconCount}");
            sb.AppendLine($"  - Izgara Hizalama Adımı: {dv.GridSpacingX}x{dv.GridSpacingY} px");
            if (dv.SampleIconPositions.Count > 0)
            {
                sb.AppendLine("  - Örnek Simge Koordinatları:");
                for (int i = 0; i < dv.SampleIconPositions.Count; i++)
                {
                    sb.AppendLine($"    - Simge {i}: {dv.SampleIconPositions[i]}");
                }
            }
            sb.AppendLine();

            sb.AppendLine("## 4. Masaüstü Çalışma Alanı Durum Analizi (Conditions A, B, C, D)");
            var diag = s.Diagnosis;
            sb.AppendLine($"- **Durum A (Çalışma Alanı Kısıtlaması):** {(diag.ConditionA_WorkAreaExcludesStrip ? "GEÇERLİ (Aktif)" : "Geçersiz")}");
            sb.AppendLine($"  - {diag.ConditionA_Details}");
            sb.AppendLine($"- **Durum B (Masaüstü Pencere Kısıtlaması):** {(diag.ConditionB_IconViewExcludesStrip ? "GEÇERLİ" : "GEÇERSİZ (Masaüstü penceresi tüm ekranı kaplıyor)")}");
            sb.AppendLine($"  - {diag.ConditionB_Details}");
            sb.AppendLine($"- **Durum C (Explorer Eski Sınır Koruma Mantığı):** {(diag.ConditionC_ExplorerRetainsOlderBoundary ? "GEÇERLİ (Kısıtlayıcı)" : "Serbest")}");
            sb.AppendLine($"  - {diag.ConditionC_Details}");
            sb.AppendLine($"- **Durum D (Kabuk Simgesi Yerleşim Engeli):** {(diag.ConditionD_ShellPreventsIconPlacement ? "GEÇERLİ (Engelleniyor)" : "Engellenmiyor")}");
            sb.AppendLine($"  - {diag.ConditionD_Details}");
            sb.AppendLine();

            return sb.ToString();
        }

        public string GenerateComparisonReport(DiagnosticComparison c)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# YCHTTOPYMIW11 Öncesi ve Sonrası Karşılaştırma Raporu");
            sb.AppendLine($"Tarih: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine();

            sb.AppendLine("## Gözlemlenen Değişiklikler");
            if (c.Observations.Count == 0)
            {
                sb.AppendLine("- Durum değişikliği algılanmadı.");
            }
            else
            {
                foreach (var obs in c.Observations)
                {
                    sb.AppendLine($"- {obs}");
                }
            }
            sb.AppendLine();

            sb.AppendLine("---");
            sb.AppendLine("### İŞLEM ÖNCESİ DURUM");
            sb.AppendLine(GenerateSnapshotReport(c.Before));

            sb.AppendLine("---");
            sb.AppendLine("### İŞLEM SONRASI DURUM");
            sb.AppendLine(GenerateSnapshotReport(c.After));

            return sb.ToString();
        }
    }
}

