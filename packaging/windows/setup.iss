#define MyAppName "DocxDiffTool"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "wys"
#define MyAppURL "https://github.com/alpha-beta-wang/DocxDiffTool"
#define MyAppExeName "DocxDiffTool.exe"
#define MyAppIcon "..\..\dotnet-app\logo.ico"

[Setup]
AppId={{B3F1E8D2-9A5C-4F7E-A1B6-D4C8E9F2A3D7}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=..\..\dotnet-app\dist\installer\windows
OutputBaseFilename=DocxDiffTool_Setup
SetupIconFile={#MyAppIcon}
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequiredOverridesAllowed=dialog
ArchitecturesInstallIn64BitMode=x64compatible

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"

[Files]
Source: "..\..\dotnet-app\dist\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs; Excludes: "installer\*"

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
