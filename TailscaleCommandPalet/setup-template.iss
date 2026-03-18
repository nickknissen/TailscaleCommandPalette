#define MyAppName "TailscaleCommandPalet"
#define MyAppDisplayName "Tailscale Command Palette"
#define MyAppVersion "{{VERSION}}"
#define MyAppPublisher "nickknissen"
#define MyAppURL "https://github.com/nickknissen/TailscaleCommandPalet"
#define MyAppExeName "TailscaleCommandPalet.exe"
#define MyCLSID "{{d4e5f6a7-b8c9-4d0e-a1b2-c3d4e5f6a7b8}}"
#define MyPlatform "{{PLATFORM}}"

[Setup]
AppId={{D4E5F6A7-B8C9-4D0E-A1B2-C3D4E5F6A7B8}
AppName={#MyAppDisplayName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
DefaultDirName={localappdata}\{#MyAppName}
DefaultGroupName={#MyAppDisplayName}
DisableProgramGroupPage=yes
OutputBaseFilename={#MyAppName}-Setup-{#MyAppVersion}-{#MyPlatform}
OutputDir=bin\Release\installer
Compression=lzma
SolidCompression=yes
PrivilegesRequired=lowest
ArchitecturesAllowed={#MyPlatform}
ArchitecturesInstallIn64BitMode={#MyPlatform}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "bin\Release\win-{#MyPlatform}\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Registry]
Root: HKCU; Subkey: "SOFTWARE\Classes\CLSID\{#MyCLSID}"; ValueType: string; ValueName: ""; ValueData: "{#MyAppName}"; Flags: uninsdeletekey
Root: HKCU; Subkey: "SOFTWARE\Classes\CLSID\{#MyCLSID}\LocalServer32"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName}"; Flags: uninsdeletekey

[Icons]
Name: "{group}\{#MyAppDisplayName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppDisplayName}"; Filename: "{uninstallexe}"

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
