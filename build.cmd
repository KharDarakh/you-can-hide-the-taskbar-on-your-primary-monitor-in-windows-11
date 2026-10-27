@echo off
REM ============================================================
REM YCHTTOPYMIW11 Build & Publish Script
REM You Can Hide The Taskbar On Your Primary Monitor In Windows 11
REM ============================================================
REM Usage:
REM   build.cmd                  - Standalone single-file publish (Default)
REM   build.cmd publish          - Standalone single-file publish
REM   build.cmd debug            - Debug build
REM   build.cmd release          - Release build
REM   build.cmd test             - Run all tests
REM   build.cmd clean            - Clean all outputs
REM ============================================================

setlocal enabledelayedexpansion

REM Detect Windows UI Language (tr vs en/other)
set "SYS_LANG=en"
for /f "tokens=*" %%i in ('powershell -NoProfile -Command "(Get-UICulture).TwoLetterISOLanguageName" 2^>nul') do (
    set "SYS_LANG=%%i"
)
if /i "%SYS_LANG%"=="tr" (
    set "IS_TR=1"
) else (
    set "IS_TR=0"
)

set SOLUTION=YCHTTOPYMIW11.sln
set PUBLISH_DIR=publish
set RUNTIME=win-x64
set "DOTNET="

:check_dotnet
REM 1. Check if dotnet is directly on PATH
where dotnet >nul 2>&1
if %ERRORLEVEL% equ 0 (
    set "DOTNET=dotnet"
    goto :dotnet_found
)

REM 2. Check standard 64-bit Program Files installation
if exist "%ProgramFiles%\dotnet\dotnet.exe" (
    set "DOTNET=%ProgramFiles%\dotnet\dotnet.exe"
    goto :dotnet_found
)

REM 3. Check 32-bit Program Files installation
if exist "%ProgramFiles(x86)%\dotnet\dotnet.exe" (
    set "DOTNET=%ProgramFiles(x86)%\dotnet\dotnet.exe"
    goto :dotnet_found
)

REM 4. Check Local AppData portable/user installation
if exist "%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe" (
    set "DOTNET=%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe"
    goto :dotnet_found
)

REM If .NET 8.0 SDK is missing, prompt user
echo.
if "%IS_TR%"=="1" (
    echo ============================================================
    echo [UYARI] .NET 8.0 SDK sisteminizde bulunamadi!
    echo Projeyi derlemek ve test etmek icin .NET 8.0 SDK gereklidir.
    echo ============================================================
    echo.
    set /p "USER_CHOICE=Indirme sayfasini acmak istiyor musunuz? (E/H) [E]: "
    if "!USER_CHOICE!"=="" set "USER_CHOICE=E"
    if /i "!USER_CHOICE!"=="E" goto :open_browser_tr
    if /i "!USER_CHOICE!"=="Evet" goto :open_browser_tr
    echo.
    echo [BILGI] Islem iptal edildi.
    exit /b 1
) else (
    echo ============================================================
    echo [WARNING] .NET 8.0 SDK was not found on your system!
    echo .NET 8.0 SDK is required to build and test this project.
    echo ============================================================
    echo.
    set /p "USER_CHOICE=Would you like to open the download page? (Y/N) [Y]: "
    if "!USER_CHOICE!"=="" set "USER_CHOICE=Y"
    if /i "!USER_CHOICE!"=="Y" goto :open_browser_en
    if /i "!USER_CHOICE!"=="Yes" goto :open_browser_en
    echo.
    echo [INFO] Operation cancelled by user.
    exit /b 1
)

:open_browser_tr
echo.
echo Tarayici aciliyor: https://dotnet.microsoft.com/download/dotnet/8.0
start https://dotnet.microsoft.com/download/dotnet/8.0
echo Lutfen acilan sayfadan .NET 8.0 SDK (x64) yukleyicisini indirip kurun.
echo.
:wait_install_tr
set /p "INSTALLED_CHOICE=SDK kurulumunu tamamladiniz mi? (E/H): "
if /i "!INSTALLED_CHOICE!"=="E" goto :reload_path_and_recheck
if /i "!INSTALLED_CHOICE!"=="Evet" goto :reload_path_and_recheck
if /i "!INSTALLED_CHOICE!"=="H" (
    echo.
    echo [BILGI] Kurulum tamamlanmadigi icin derleme islemi durduruldu.
    exit /b 1
)
if /i "!INSTALLED_CHOICE!"=="Hayir" (
    echo.
    echo [BILGI] Kurulum tamamlanmadigi icin derleme islemi durduruldu.
    exit /b 1
)
echo Lutfen 'E' (Evet) veya 'H' (Hayir) girin.
goto :wait_install_tr

:open_browser_en
echo.
echo Opening browser: https://dotnet.microsoft.com/download/dotnet/8.0
start https://dotnet.microsoft.com/download/dotnet/8.0
echo Please download and install the .NET 8.0 SDK (x64) installer.
echo.
:wait_install_en
set /p "INSTALLED_CHOICE=Have you completed the SDK installation? (Y/N): "
if /i "!INSTALLED_CHOICE!"=="Y" goto :reload_path_and_recheck
if /i "!INSTALLED_CHOICE!"=="Yes" goto :reload_path_and_recheck
if /i "!INSTALLED_CHOICE!"=="N" (
    echo.
    echo [INFO] Build stopped because SDK installation was not completed.
    exit /b 1
)
if /i "!INSTALLED_CHOICE!"=="No" (
    echo.
    echo [INFO] Build stopped because SDK installation was not completed.
    exit /b 1
)
echo Please enter 'Y' (Yes) or 'N' (No).
goto :wait_install_en

:reload_path_and_recheck
REM Refresh PATH environment variables from the registry
for /f "tokens=2* delims= " %%a in ('reg query "HKLM\System\CurrentControlSet\Control\Session Manager\Environment" /v Path 2^>nul') do set "PATH=%%b;!PATH!"
for /f "tokens=2* delims= " %%a in ('reg query "HKCU\Environment" /v Path 2^>nul') do set "PATH=%%b;!PATH!"
goto :check_dotnet

:dotnet_found
REM Auto-generate logo assets if missing
if not exist "src\YCHTTOPYMIW11.App\Assets\app.ico" (
    if "%IS_TR%"=="1" (
        echo [BILGI] Uygulama logosu ve simge dosyalari hazirlaniyor...
    ) else (
        echo [INFO] Generating application logo and icon assets...
    )
    powershell -ExecutionPolicy Bypass -File generate_logo.ps1
)

REM Command Dispatcher (Default is publish: Standalone single-file .exe)
if "%1"=="" goto :publish
if /i "%1"=="publish" goto :publish
if /i "%1"=="debug" goto :build_debug
if /i "%1"=="release" goto :build_release
if /i "%1"=="test" goto :test
if /i "%1"=="clean" goto :clean

if "%IS_TR%"=="1" (
    echo [HATA] Bilinmeyen komut: %1
    echo Kullanim: build.cmd [publish^|debug^|release^|test^|clean]
) else (
    echo [ERROR] Unknown command: %1
    echo Usage: build.cmd [publish^|debug^|release^|test^|clean]
)
exit /b 1

:build_debug
if "%IS_TR%"=="1" (
    echo === Debug Derlemesi Baslatiliyor ===
) else (
    echo === Starting Debug Build ===
)
"%DOTNET%" build %SOLUTION% -c Debug --nologo
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%
echo.
if "%IS_TR%"=="1" (
    echo Debug derlemesi basarili. Klasor aciliyor...
) else (
    echo Debug build successful. Opening folder...
)
start "" explorer.exe "%CD%\src\YCHTTOPYMIW11.App\bin\Debug\net8.0-windows"
goto :eof

:build_release
if "%IS_TR%"=="1" (
    echo === Release Modunda Optimize Derleme Baslatiliyor ===
) else (
    echo === Starting Release Build ===
)
"%DOTNET%" build %SOLUTION% -c Release --nologo
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%
echo.
if "%IS_TR%"=="1" (
    echo Release derlemesi basarili. Klasor aciliyor...
) else (
    echo Release build successful. Opening folder...
)
start "" explorer.exe "%CD%\src\YCHTTOPYMIW11.App\bin\Release\net8.0-windows"
goto :eof

:publish
if "%IS_TR%"=="1" (
    echo === YCHTTOPYMIW11 Tek Dosya Bagimsiz Yayinlama (Publish) ===
) else (
    echo === YCHTTOPYMIW11 Single-File Self-Contained Publish ===
)
"%DOTNET%" publish src\YCHTTOPYMIW11.App\YCHTTOPYMIW11.App.csproj -c Release -r %RUNTIME% --self-contained true -p:PublishSingleFile=true -o %PUBLISH_DIR% --nologo
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%
echo.
if "%IS_TR%"=="1" (
    echo Yayinlama tamamlandi: %PUBLISH_DIR%\YCHTTOPYMIW11.exe
    echo Klasor aciliyor...
) else (
    echo Publish completed: %PUBLISH_DIR%\YCHTTOPYMIW11.exe
    echo Opening folder...
)
start "" explorer.exe "%CD%\%PUBLISH_DIR%"
goto :eof

:test
if "%IS_TR%"=="1" (
    echo === Birim Testleri Calistiriliyor ===
) else (
    echo === Running Unit Tests ===
)
"%DOTNET%" test %SOLUTION% -c Debug --nologo --verbosity normal
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%
echo.
if "%IS_TR%"=="1" (
    echo Tum birim testleri basarili.
) else (
    echo All unit tests passed.
)
goto :eof

:clean
if "%IS_TR%"=="1" (
    echo === Derleme Ciktilari ve Onbellek Temizleniyor ===
) else (
    echo === Cleaning Build Outputs and Caches ===
)
"%DOTNET%" clean %SOLUTION% --nologo
if exist "%PUBLISH_DIR%" rd /s /q "%PUBLISH_DIR%"
for /d /r . %%d in (bin,obj) do @if exist "%%d" rd /s /q "%%d"
if "%IS_TR%"=="1" (
    echo Temizlik tamamlandi.
) else (
    echo Clean completed.
)
goto :eof
