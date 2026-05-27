@echo off
setlocal
cd /d "%~dp0..\..\dotnet-app"

set ISCC=%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe

:: ---- x64 ----
echo [x64 1/3] Publishing...
dotnet publish -c Release -r win-x64 -o dist\win-x64 --self-contained true -p:PublishSingleFile=true
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo [x64 2/3] Building portable zip...
if not exist dist\installer\windows mkdir dist\installer\windows
zip -j "dist\installer\windows\DocxDiffTool_x64_Portable.zip" dist\win-x64\DocxDiffTool.exe dist\win-x64\Photino.Native.dll dist\win-x64\WebView2Loader.dll dist\win-x64\aspnetcorev2_inprocess.dll
if %ERRORLEVEL% neq 0 (
    echo WARNING: zip not found, trying PowerShell...
    powershell -Command "Compress-Archive -Path dist\win-x64\DocxDiffTool.exe,dist\win-x64\Photino.Native.dll,dist\win-x64\WebView2Loader.dll,dist\win-x64\aspnetcorev2_inprocess.dll -DestinationPath dist\installer\windows\DocxDiffTool_x64_Portable.zip -Force"
)

echo [x64 3/3] Building installer...
if exist "%ISCC%" (
    "%ISCC%" /DMyArch="x64" "%~dp0setup.iss"
) else (
    echo WARNING: Inno Setup not found, skipping installer
)

:: ---- ARM64 ----
echo.
echo [arm64 1/2] Publishing...
dotnet publish -c Release -r win-arm64 -o dist\win-arm64 --self-contained true -p:PublishSingleFile=true
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo [arm64 2/2] Building portable zip...
zip -j "dist\installer\windows\DocxDiffTool_arm64_Portable.zip" dist\win-arm64\DocxDiffTool.exe dist\win-arm64\Photino.Native.dll dist\win-arm64\WebView2Loader.dll dist\win-arm64\aspnetcorev2_inprocess.dll
if %ERRORLEVEL% neq 0 (
    powershell -Command "Compress-Archive -Path dist\win-arm64\DocxDiffTool.exe,dist\win-arm64\Photino.Native.dll,dist\win-arm64\WebView2Loader.dll,dist\win-arm64\aspnetcorev2_inprocess.dll -DestinationPath dist\installer\windows\DocxDiffTool_arm64_Portable.zip -Force"
)

echo.
echo Done!
echo   x64 Portable:  dist\installer\windows\DocxDiffTool_x64_Portable.zip
echo   x64 Installer:  dist\installer\windows\DocxDiffTool_x64_Setup.exe
echo   arm64 Portable: dist\installer\windows\DocxDiffTool_arm64_Portable.zip
