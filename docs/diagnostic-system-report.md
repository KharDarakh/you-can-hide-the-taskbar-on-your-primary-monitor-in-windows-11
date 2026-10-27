# YCHTTOPYMIW11 Windows 11 Sistem Tanılama Raporu
Rapor Oluşturulma Zamanı (UTC): 2026-10-27 04:58:57

## 1. Algılanan Monitörler ve Çalışma Alanları
- **DISPLAY1 (Ana Ekran)** (\\.\DISPLAY1)
  - Ana Ekran: Evet
  - Tam Ekran Çözünürlüğü: (0, 0, 2560, 1440) [2560x1440] (2560x1440)
  - Bildirilen Çalışma Alanı: (0, 0, 2560, 1392) [2560x1392] (2560x1392)
  - Görev Çubuğu Ayrılmış Alanı: 48 px
  - DPI: 96x96 (Ölçek: %100)
- **DISPLAY2 (İkincil Ekran)** (\\.\DISPLAY2)
  - Ana Ekran: Hayır
  - Tam Ekran Çözünürlüğü: (-1920, 0, 0, 1080) [1920x1080] (1920x1080)
  - Bildirilen Çalışma Alanı: (-1920, 0, 0, 1032) [1920x1032] (1920x1032)
  - Görev Çubuğu Ayrılmış Alanı: 48 px
  - DPI: 96x96 (Ölçek: %100)

## 2. Görev Çubuğu Pencereleri
- **Shell_TrayWnd** (Rol: Primary, HWND: 0x3041A)
  - Görünürlük Durumu: Görünür (Visible)
  - Konum ve Boyut: (0, 1392, 2560, 1440) [2560x48] (2560x48)
  - Ait Olduğu Ekran: \\.\DISPLAY1 (HMON: 0x10048)
  - Pencere Stilleri: Style=0x96000000, ExStyle=0x00000088
- **Shell_SecondaryTrayWnd** (Rol: Secondary, HWND: 0x303E8)
  - Görünürlük Durumu: Görünür (Visible)
  - Konum ve Boyut: (-1920, 1032, 0, 1080) [1920x48] (1920x48)
  - Ait Olduğu Ekran: \\.\DISPLAY2 (HMON: 0x20001)
  - Pencere Stilleri: Style=0x96000000, ExStyle=0x00000088

## 3. Masaüstü Simge Görünümü (Shell View Hierarchy)
- **Progman HWND:** 0x201D0
- **SHELLDLL_DefView HWND:** 0x210934
  - Sınırlar: (-1920, 0, 2560, 1440) [4480x1440], İstemci Alanı: (0, 0, 4480, 1440) [4480x1440]
- **SysListView32 HWND:** 0x301D6
  - Sınırlar: (-1920, 0, 2560, 1440) [4480x1440], İstemci Alanı: (0, 0, 4480, 1440) [4480x1440]
  - Stil: 0x56003A40
  - Toplam Masaüstü Simgesi Sayısı: 9
  - Izgara Hizalama Adımı: 75x99 px
  - Örnek Simge Koordinatları:
    - Simge 0: (1933, 2)
    - Simge 1: (1933, 101)
    - Simge 2: (1933, 200)
    - Simge 3: (1933, 299)
    - Simge 4: (1933, 398)
    - Simge 5: (1933, 497)
    - Simge 6: (1933, 596)
    - Simge 7: (1933, 695)
    - Simge 8: (1933, 794)

## 4. Masaüstü Çalışma Alanı Durum Analizi (Conditions A, B, C, D)
- **Durum A (Çalışma Alanı Kısıtlaması):** GEÇERLİ (Aktif)
  - Primary display excludes bottom 48px (Monitor: 1440px, WorkArea: 1392px).
- **Durum B (Masaüstü Pencere Kısıtlaması):** GEÇERSİZ (Masaüstü penceresi tüm ekranı kaplıyor)
  - SHELLDLL_DefView spans full virtual height (1440px), covering monitor bottom (1440 >= 1440).
- **Durum C (Explorer Eski Sınır Koruma Mantığı):** GEÇERLİ (Kısıtlayıcı)
  - Explorer icon placement clamps icon bounding boxes to fit inside rcWork.Bottom (clamping to Row 13 / Y=1289 with 99px grid).
- **Durum D (Kabuk Simgesi Yerleşim Engeli):** GEÇERLİ (Engelleniyor)
  - Desktop icon alignment to grid prevents placement in bottom 48px while taskbar work area is excluded.

