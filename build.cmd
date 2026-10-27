@echo off
REM ============================================================
REM YCHTTOPYMIW11 Build & Publish Script
REM You Can Hide The Taskbar On Your Primary Monitor In Windows 11
REM ============================================================
REM Usage:
REM   build.cmd                  - Debug build
REM   build.cmd release          - Release build
REM   build.cmd publish          - Self-contained publish
REM   build.cmd test             - Run all tests
REM   build.cmd clean            - Clean all outputs
REM ============================================================

setlocal enabledelayedexpansion

set DOTNET=dotnet
set SOLUTION=YCHTTOPYMIW11.sln
set CONFIGURATION=Debug
set PUBLISH_DIR=publish
set RUNTIME=win-x64

REM Check for dotnet
where dotnet >nul 2>&1
if %ERRORLEVEL% neq 0 (
    if exist "%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe" (
        set DOTNET=%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe
    ) else (
        echo [HATA] dotnet bulunamadi. .NET 8.0 SDK yukleyin.
        exit /b 1
    )
)

if not exist "src\YCHTTOPYMIW11.App\Assets\app.ico" (
    echo [BILGI] Uygulama logosu ve simge dosyalari hazirlaniyor...
    powershell -ExecutionPolicy Bypass -File generate_logo.ps1
)

if "%1"=="" goto :build_debug
if /i "%1"=="release" goto :build_release
if /i "%1"=="publish" goto :publish
if /i "%1"=="test" goto :test
if /i "%1"=="clean" goto :clean
echo Bilinmeyen komut: %1
echo Kullanim: build.cmd [release^|publish^|test^|clean]
exit /b 1

:build_debug
echo === Debug Build ===
%DOTNET% build %SOLUTION% -c Debug --nologo
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%
echo.
echo Debug build basarili.
goto :eof

:build_release
echo === Release Build ===
%DOTNET% build %SOLUTION% -c Release --nologo
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%
echo.
echo Release build basarili.
goto :eof

:publish
echo === YCHTTOPYMIW11 Single-File Self-Contained Publish ===
%DOTNET% publish src\YCHTTOPYMIW11.App\YCHTTOPYMIW11.App.csproj -c Release -r %RUNTIME% --self-contained true -p:PublishSingleFile=true -o %PUBLISH_DIR% --nologo
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%
echo.
echo Yayinlama tamamlandi: %PUBLISH_DIR%\YCHTTOPYMIW11.exe
goto :eof

:test
echo === Unit Tests ===
%DOTNET% test %SOLUTION% -c Debug --nologo --verbosity normal
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%
echo.
echo Tum testler basarili.
goto :eof

:clean
echo === Cleaning ===
%DOTNET% clean %SOLUTION% --nologo
if exist "%PUBLISH_DIR%" rd /s /q "%PUBLISH_DIR%"
for /d /r . %%d in (bin,obj) do @if exist "%%d" rd /s /q "%%d"
echo Temizlik tamamlandi.
goto :eof
