# Testing Documentation & Hardware Test Plan — YCHTTOPYMIW11

## 1. Automated Test Suite

YCHTTOPYMIW11 includes an automated unit test suite (`YCHTTOPYMIW11.Tests`) developed using xUnit and .NET 8.

### 1.1 Test Coverage Overview

| Test Class | Purpose | Status |
| :--- | :--- | :--- |
| `GeometryTests` | Verifies `RECT` width, height, negative coordinates, empty detection, equality, and hash code calculations. | **Passed** |
| `ConfigurationTests` | Verifies `AppConfig` defaults, deep cloning of `MonitorPolicyConfig`, JSON serialization/deserialization, and backup fallback. | **Passed** |
| `DiagnosticReportTests` | Verifies snapshot report formatting, condition evaluation text, and comparison report generation. | **Passed** |
| `LiveSystemDiagnosticTest` | Connects to the host system via `DesktopSessionHelper`, captures live monitor/taskbar metrics, and validates output. | **Passed** |

To execute the test suite:
```powershell
dotnet test --no-restore
```

---

## 2. Manual Hardware Test Plan

The following manual test matrix covers scenarios that cannot be fully automated without physical display manipulation and human interaction.

> [!IMPORTANT]
> Hardware-specific manual test cases must be physically verified by a test engineer on dual-monitor Windows 11 hardware.

### Test Matrix

#### Scenario 1: Dual Monitor Setup — Monitor 1 as Primary
- **Preconditions**: Two physical monitors connected. Display 1 set as "Main Display" in Windows Display Settings.
- **Action**: In YCHTTOPYMIW11, set Monitor 1 to `PermanentlyHidden` and Monitor 2 to `AlwaysVisible`. Click **İlkeleri Uygula**.
- **Expected Outcome**:
  - Monitor 1 taskbar becomes completely invisible.
  - Monitor 2 taskbar remains fully visible with Start menu, pinned apps, clock, and system tray.
  - Moving the mouse cursor to the bottom edge of Monitor 1 does **not** reveal or flash the taskbar.

#### Scenario 2: Dual Monitor Setup — Monitor 2 as Primary
- **Preconditions**: In Windows Display Settings, set Monitor 2 as "Main Display".
- **Action**: In YCHTTOPYMIW11, set Monitor 2 to `PermanentlyHidden` and Monitor 1 to `AlwaysVisible`. Click **İlkeleri Uygula**.
- **Expected Outcome**:
  - Monitor 2 taskbar becomes completely invisible.
  - Monitor 1 taskbar remains visible and functional.

#### Scenario 3: Mixed Resolutions
- **Preconditions**: Monitor 1 set to 2560x1440; Monitor 2 set to 1920x1080.
- **Action**: Apply `PermanentlyHidden` to Monitor 1.
- **Expected Outcome**: Taskbar hides cleanly without affecting Monitor 2's bounds.

#### Scenario 4: Different DPI Scaling
- **Preconditions**: Monitor 1 set to 125% or 150% DPI scale; Monitor 2 set to 100%.
- **Action**: Open YCHTTOPYMIW11, verify DPI readings on cards, apply policy.
- **Expected Outcome**: YCHTTOPYMIW11 scales properly without blurry text (PerMonitorV2); taskbar hides correctly.

#### Scenario 5: Bottom-Edge Mouse Activation Resistance
- **Preconditions**: Taskbar hidden on Monitor 1.
- **Action**: Move mouse rapidly to the bottom 1-pixel boundary of Monitor 1. Click, double-click, and drag across the edge.
- **Expected Outcome**: No taskbar reveal, no visual flicker, no slide animation.

#### Scenario 6: Start Menu Interactions on Visible Monitor
- **Preconditions**: Monitor 1 taskbar hidden, Monitor 2 taskbar visible.
- **Action**: Click Start button on Monitor 2; press `Win` key on keyboard.
- **Expected Outcome**: Start menu opens correctly on the active monitor without revealing Monitor 1's taskbar.

#### Scenario 7: Windows Explorer Restart Recovery
- **Preconditions**: Policies applied with Monitor 1 taskbar hidden.
- **Action**: Open Command Prompt / PowerShell and execute `taskkill /f /im explorer.exe && start explorer.exe`.
- **Expected Outcome**: YCHTTOPYMIW11 intercepts `TaskbarCreated`, waits for shell re-initialization, and re-asserts the `PermanentlyHidden` policy automatically.

#### Scenario 8: System Sleep and Resume
- **Preconditions**: Monitor 1 taskbar hidden.
- **Action**: Put Windows to Sleep, wait 10 seconds, wake system up.
- **Expected Outcome**: After login, Monitor 1 taskbar remains hidden and Monitor 2 remains visible.

#### Scenario 9: Display Disconnect & Reconnect (Hot-unplug)
- **Preconditions**: Monitor 1 taskbar hidden.
- **Action**: Unplug HDMI/DisplayPort cable of Monitor 2, wait 5 seconds, reconnect.
- **Expected Outcome**: YCHTTOPYMIW11 intercepts `WM_DISPLAYCHANGE`, refreshes display handles, and maintains configured policies.

#### Scenario 10: Display Arrangement Shift
- **Preconditions**: In Windows Display Settings, drag Monitor 2 from left to right side of Monitor 1.
- **Action**: Apply arrangement change.
- **Expected Outcome**: YCHTTOPYMIW11 adapts coordinates dynamically; hidden taskbar stays hidden.

#### Scenario 11: Desktop Icon Placement in Bottom Area
- **Preconditions**: Monitor 1 taskbar hidden, work area expansion enabled.
- **Action**: Disable "Align icons to grid" (Right click desktop -> View -> uncheck Align icons to grid). Drag desktop icon into bottom 48-pixel strip.
- **Expected Outcome**: Desktop icon stays in the bottom 48-pixel area without snapping upward. Maximize an application window (e.g. Notepad) on Monitor 1; window fills the full display height (1440px).

#### Scenario 12: System Reboot & Persistence
- **Preconditions**: "Windows açılışında otomatik başlat" checked in YCHTTOPYMIW11 settings.
- **Action**: Restart computer.
- **Expected Outcome**: YCHTTOPYMIW11 launches automatically on boot, applies saved policies from `%LOCALAPPDATA%\YCHTTOPYMIW11\config.json`, and starts minimized to system tray.

