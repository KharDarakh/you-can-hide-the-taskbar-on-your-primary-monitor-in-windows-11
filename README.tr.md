<p align="center">
  <img src="src/YCHTTOPYMIW11.App/Assets/app.png" width="80" height="80" alt="YCHTTOPYMIW11 Logo" />
</p>

# YCHTTOPYMIW11
### You Can Hide The Taskbar On Your Primary Monitor In Windows 11

<p align="center">
  <b>Dil / Language:</b>
  <a href="README.tr.md">🇹🇷 Türkçe</a> |
  <a href="README.md">🇬🇧 English</a>
</p>

---

Windows 11'de çoklu monitör kullanırken ana monitördeki (veya seçilen herhangi bir ekrandaki) görev çubuğunu fare kenar tetikleyicisi olmadan tamamen gizler ve masaüstü çalışma alanını tam ekrana genişletir.

## Özellikler

- **Bağımsız Ekran İlkeleri**: İstediğiniz ekranda görev çubuğunu gizleyin, diğerlerinde açık bırakın.
- **Gerçek Tam Ekran**: Görev çubuğunun 48 piksellik alanı serbest bırakılır, büyütülen pencereler ekranın en altına kadar uzanır.
- **Fare Tetikleyicisi Yok**: Fare alt kenara geldiğinde görev çubuğu açılmaz, kaymaz veya yanıp sönmez.
- **Otomatik Kurtarma**: Windows Gezgini (Explorer) yeniden başladığında ilkeleri otomatik olarak korur.

## Sistem Gereksinimleri

- Windows 11 (64-bit)
- 2 veya daha fazla monitör
- Derleme için: .NET 8.0 SDK (x64)

## Derleme (`build.cmd`)

Projeyi derlemek için kök dizindeki `build.cmd` dosyasını çalıştırmanız yeterlidir:

```cmd
REM Bağımsız tek dosya (.exe) üretir ve klasörü açar:
build.cmd

REM Testleri çalıştırmak için:
build.cmd test
```

> **Not:** .NET 8.0 SDK kurulu değilse `build.cmd` sizi indirme sayfasına yönlendirir ve kurulum sonrası derlemeye kaldığı yerden devam eder.

## Kullanım Kılavuzu

1. `publish\YCHTTOPYMIW11.exe` uygulamasını çalıştırın.
2. Görev çubuğunu gizlemek istediğiniz ekran için **Kalıcı Olarak Gizli** seçeneğini seçin.
3. Diğer ekranlar için **Her Zaman Görünür** bırakın.
4. **İlkeleri Uygula** butonuna tıklayın.

## Acil Durum Kurtarma

Görev çubuklarını varsayılan Windows ayarlarına geri döndürmek için:
- Uygulama içindeki veya sistem tepsisindeki (sağ tık) **Varsayılanları Geri Yükle** butonuna tıklayın.
- Ya da PowerShell'de Gezgin'i yeniden başlatın:
  ```powershell
  Stop-Process -Name explorer -Force
  ```

## Belgeler

- [Teknik Araştırma](docs/technical-research.md)
- [Mimari Tasarım](docs/architecture.md)
- [Donanım Test Planı](docs/testing.md)
- [Sistem Tanılama Raporu](docs/diagnostic-system-report.md)

## Lisans

Bu proje [MIT Lisansı](LICENSE) altında lisanslanmıştır.
