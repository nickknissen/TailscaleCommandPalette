; Inno Setup Script for Tailscale Command Palette
; Used by build-exe.ps1 to produce per-architecture installers for WinGet releases.

#define AppVersion "2.0.0"

[Setup]
AppId={{d4e5f6a7-b8c9-4d0e-a1b2-c3d4e5f6a7b8}
AppName=Tailscale Command Palette
AppVersion={#AppVersion}
AppPublisher=Nick Knissen
DefaultDirName={localappdata}\TailscaleCommandPalette
DefaultGroupName=Tailscale Command Palette
DisableProgramGroupPage=yes
OutputDir=bin\Release\installer
OutputBaseFilename=TailscaleCommandPalette-Setup-{#AppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
MinVersion=10.0.19041

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "bin\Release\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Tailscale Command Palette"; Filename: "{app}\TailscaleCommandPalette.exe"
Name: "{group}\Uninstall Tailscale Command Palette"; Filename: "{uninstallexe}"

[Registry]
Root: HKCU; Subkey: "SOFTWARE\Classes\CLSID\{{d4e5f6a7-b8c9-4d0e-a1b2-c3d4e5f6a7b8}}"; ValueType: string; ValueData: "TailscaleCommandPalette"; Flags: uninsdeletekey
Root: HKCU; Subkey: "SOFTWARE\Classes\CLSID\{{d4e5f6a7-b8c9-4d0e-a1b2-c3d4e5f6a7b8}}\LocalServer32"; ValueType: string; ValueData: '"{app}\TailscaleCommandPalette.exe" -RegisterProcessAsComServer'; Flags: uninsdeletekey

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
