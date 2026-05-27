@echo off
setlocal
cd /d "%~dp0..\..\dotnet-app"

echo [1/3] Publishing...
dotnet publish -c Release -r win-x64 -o dist --self-contained true -p:PublishSingleFile=true
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo [2/3] Building portable zip...
if not exist dist\installer\windows mkdir dist\installer\windows
zip -j "dist\installer\windows\DocxDiffTool_Portable.zip" dist\DocxDiffTool.exe dist\Photino.Native.dll dist\WebView2Loader.dll dist\aspnetcorev2_inprocess.dll
if %ERRORLEVEL% neq 0 (
    echo WARNING: zip not found, trying PowerShell...
    powershell -Command "Compress-Archive -Path dist\DocxDiffTool.exe,dist\Photino.Native.dll,dist\WebView2Loader.dll,dist\aspnetcorev2_inprocess.dll -DestinationPath dist\installer\windows\DocxDiffTool_Portable.zip -Force"
)

echo [3/3] Building installer...
set ISCC=%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe
if not exist "%ISCC%" (
    echo ERROR: Inno Setup not found at %ISCC%
    exit /b 1
)
"%ISCC%" "%~dp0setup.iss"

echo.
echo Done!
echo   Portable: dist\installer\windows\DocxDiffTool_Portable.zip
echo   Installer: dist\installer\windows\DocxDiffTool_Setup.exe
