<p align="center">
  <img src="src/YCHTTOPYMIW11.App/Assets/app.png" width="96" height="96" alt="YCHTTOPYMIW11 Logo" />
</p>

# YCHTTOPYMIW11
### You Can Hide The Taskbar On Your Primary Monitor In Windows 11

<p align="center">
  <b>Language / Dil:</b>
  <a href="README.md">🇬🇧 English</a> |
  <a href="README.tr.md">🇹🇷 Türkçe</a>
</p>

---

[![Platform](https://img.shields.io/badge/Platform-Windows%2011%20(64--bit)-blue.svg)](https://microsoft.com/windows)
[![Framework](https://img.shields.io/badge/.NET-8.0%20LTS-purple.svg)](https://dotnet.microsoft.com/)
[![Architecture](https://img.shields.io/badge/Architecture-WPF%20%7C%20MVVM%20%7C%20Win32-green.svg)]()
[![Tests](https://img.shields.io/badge/Unit%20Tests-11%20Passed-brightgreen.svg)]()
[![License](https://img.shields.io/badge/License-MIT-orange.svg)]()

**GitHub Repository:** `you-can-hide-the-taskbar-on-your-primary-monitor-in-windows-11`

**YCHTTOPYMIW11** is a native Windows 11 multi-monitor utility designed to permanently hide the taskbar on your primary display (or any selected screen) without mouse edge-reveal triggers, while automatically expanding the desktop work area to true full screen.

Built with modern C# / .NET 8.0 LTS and direct Win32 API integration.

---

## 1. Key Features

- **Independent Taskbar Policies**: Configure individual policies per monitor:
  - **Always Visible**: Default Windows behavior (taskbar remains visible).
  - **Permanently Hidden**: Completely hides the taskbar; moving the mouse cursor to the bottom edge will NOT trigger, slide, or flash the taskbar.
- **Automatic Desktop Work Area Expansion**:
  - Automatically releases the 48px reserved taskbar area when hidden.
  - Maximized windows extend seamlessly to the bottom edge of the display with zero gap or black bar.
  - Desktop icons can freely occupy the bottom area.
  - Preserves desktop icon arrangement without resetting or displacing items.
- **Modern Windows 11 Tabbed UI**:
  - **Taskbars Tab**: Streamlined monitor cards displaying live resolution, DPI scale, current visibility state, and policy dropdowns.
  - **App & Recovery Settings Tab**: Startup options, Explorer crash auto-recovery, and system diagnostics.
  - **Interactive Accordion Title Header**: Dynamic kinetic typography header featuring the acronym `YCHTTOPYMIW11`; hovering reveals each full word smoothly pushing adjacent letters.
- **Event-Driven Resilience & Auto-Recovery**:
  - **Explorer Crash / Restart Recovery**: Listens for the `TaskbarCreated` message to automatically re-enforce configured policies if Windows Explorer restarts.
  - **Display Topology Changes**: Responds to `WM_DISPLAYCHANGE` whenever displays are connected, disconnected, or resolutions/DPI change.
  - Zero polling loops — completely event-driven Win32 architecture for minimal CPU and memory footprint.
- **Safe Rollback**:
  - One-click restore to default Windows configuration.
  - Automatically restores work areas and taskbar visibility upon application exit or emergency recovery.

---

## 2. System Requirements

- **Operating System**: Windows 11 (All 64-bit editions)
- **Development / Build**: .NET 8.0 SDK (x64)
- **Runtime**: .NET 8.0 Desktop Runtime (or zero requirements if using the single-file self-contained publish executable)
- **Display Setup**: 2 or more physical or virtual monitors

---

## 3. Build & Deployment (`build.cmd`)

Use the root-level script **`build.cmd`** to build, test, clean, or create a standalone release package. The script automatically detects the installed .NET 8.0 SDK.

### Available Commands:

```cmd
REM 1. Build in Debug mode
build.cmd

REM 2. Build in Release mode (optimized)
build.cmd release

REM 3. Run all xUnit unit tests
build.cmd test

REM 4. Publish standalone single-file (.exe)
build.cmd publish

REM 5. Clean all build outputs and caches
build.cmd clean
```

### Standalone Executable (Publish):
Executing `build.cmd publish` generates:
- A standalone, self-contained single executable (`publish\YCHTTOPYMIW11.exe`) that runs on any Windows 11 machine without installing .NET Runtime or external dependencies.
- Output location: `publish\YCHTTOPYMIW11.exe`
- Ready for immediate distribution or portable use.

---

## 4. User Guide

1. Launch `YCHTTOPYMIW11.exe`.
2. Under the **Taskbars** tab, view your connected monitors.
3. For the monitor where you wish to hide the taskbar (e.g., Primary Display):
   - Set **Taskbar Policy** to **Permanently Hidden**.
4. For any monitor where you wish to keep the taskbar:
   - Ensure **Always Visible** is selected.
5. Click **Apply Policies** in the bottom right corner.
6. The target taskbar hides immediately, the desktop work area expands to true full screen, and other monitors retain their taskbars unaffected.

---

## 5. Application & Recovery Settings

The second tab provides system integration and resilience settings:

- **Launch on Windows Startup**: Adds an entry to `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` to start YCHTTOPYMIW11 in the background on boot.
- **Start minimized to System Tray**: Keeps the main window hidden on boot, living quietly in the notification area.
- **Re-apply policies when Windows Explorer restarts**: Captures the `TaskbarCreated` broadcast message.
- **Re-evaluate policies on display configuration changes**: Handles `WM_DISPLAYCHANGE` events.
- **Detailed System Report**: Displays Win32 window handles (`HWND`), window hierarchy, and raw coordinates for troubleshooting.

---

## 6. Emergency Recovery

To immediately restore all taskbars and work areas to Windows defaults:

### Method 1: In the Application
Click the **Restore Defaults** button at the bottom of the window.

### Method 2: System Tray Icon
Right-click the YCHTTOPYMIW11 icon in the notification tray and select **Restore Defaults**.

### Method 3: Emergency PowerShell Script
If the application is closed and you need an immediate manual reset, run this in PowerShell:

```powershell
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public class Restorer {
    [DllImport("user32.dll")] public static extern IntPtr FindWindow(string c, string t);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int c);
    [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h, IntPtr a, int x, int y, int cx, int cy, uint f);
    public static void Show() {
        IntPtr p = FindWindow("Shell_TrayWnd", null);
        if (p != IntPtr.Zero) { ShowWindow(p, 5); SetWindowPos(p, IntPtr.Zero, 0, 0, 0, 0, 0x0001|0x0002|0x0040); }
        IntPtr s = FindWindow("Shell_SecondaryTrayWnd", null);
        if (s != IntPtr.Zero) { ShowWindow(s, 5); SetWindowPos(s, IntPtr.Zero, 0, 0, 0, 0, 0x0001|0x0002|0x0040); }
    }
}
'@
[Restorer]::Show()
```

---

## 7. Project Documentation

- [Technical Research & Win32 Deep Dive](docs/technical-research.md)
- [Architecture & Design Specification](docs/architecture.md)
- [Hardware Test Plan & Scenarios](docs/testing.md)
- [Host Diagnostic System Report](docs/diagnostic-system-report.md)

---

## 8. License

This project is licensed under the [MIT License](LICENSE). Open-source and free to fork and customize.
