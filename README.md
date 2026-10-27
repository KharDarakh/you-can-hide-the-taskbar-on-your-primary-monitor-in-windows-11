<p align="center">
  <img src="src/YCHTTOPYMIW11.App/Assets/app.png" width="80" height="80" alt="YCHTTOPYMIW11 Logo" />
</p>

# YCHTTOPYMIW11
### You Can Hide The Taskbar On Your Primary Monitor In Windows 11

<p align="center">
  <b>Language / Dil:</b>
  <a href="README.md">🇬🇧 English</a> |
  <a href="README.tr.md">🇹🇷 Türkçe</a>
</p>

---

Permanently hides the taskbar on your primary display (or any selected screen) in Windows 11 without mouse edge-reveal triggers, expanding your desktop work area to true full screen.

## Features

- **Per-Monitor Policies**: Hide the taskbar on your primary display while keeping it visible on secondary screens.
- **True Full Screen**: Automatically reclaims the 48px reserved taskbar area; maximized windows extend edge-to-edge.
- **No Edge Triggers**: Moving your mouse to the bottom edge will not pop up, slide, or flash the taskbar.
- **Auto Recovery**: Automatically re-applies policies if Windows Explorer crashes or restarts.

## System Requirements

- Windows 11 (64-bit)
- 2 or more monitors
- To build: .NET 8.0 SDK (x64)

## How to Build (`build.cmd`)

Simply run `build.cmd` in the root directory:

```cmd
REM Builds standalone single-file .exe and opens output folder:
build.cmd

REM Run unit tests:
build.cmd test
```

> **Note:** If .NET 8.0 SDK is missing, `build.cmd` prompts to open the official download page and resumes compilation once installed.

## User Guide

1. Run `publish\YCHTTOPYMIW11.exe`.
2. Set the policy to **Permanently Hidden** for the display you want to hide the taskbar on.
3. Keep **Always Visible** for any other display.
4. Click **Apply Policies**.

## Emergency Recovery

To restore all taskbars to Windows defaults:
- Click **Restore Defaults** inside the application or from the system tray icon (right-click).
- Or restart Windows Explorer via PowerShell:
  ```powershell
  Stop-Process -Name explorer -Force
  ```

## Documentation

- [Technical Research](docs/technical-research.md)
- [Architecture](docs/architecture.md)
- [Hardware Test Plan](docs/testing.md)
- [System Diagnostic Report](docs/diagnostic-system-report.md)

## License

This project is licensed under the [MIT License](LICENSE).
