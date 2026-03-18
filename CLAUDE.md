# Tailscale Command Palette Extension

PowerToys Command Palette extension for Tailscale devices.

## Build & Deploy

Build the project (x64 Debug with publish/trimming):

```sh
dotnet clean TailscaleCommandPalet/TailscaleCommandPalet.csproj -c Debug -p:Platform=x64 -p:RuntimeIdentifier=win-x64
dotnet publish TailscaleCommandPalet/TailscaleCommandPalet.csproj -c Debug -p:Platform=x64 -p:RuntimeIdentifier=win-x64
```

Copy the publish output into the AppX folder (which is what the registered package points to):

```sh
cp bin/Debug/net10.0-windows10.0.26100.0/win-x64/publish/TailscaleCommandPalet.dll bin/x64/Debug/net10.0-windows10.0.26100.0/win-x64/AppX/TailscaleCommandPalet.dll
cp bin/Debug/net10.0-windows10.0.26100.0/win-x64/publish/TailscaleCommandPalet.exe bin/x64/Debug/net10.0-windows10.0.26100.0/win-x64/AppX/TailscaleCommandPalet.exe
```

Register (or re-register) the extension:

```powershell
Get-AppxPackage -Name TailscaleCommandPalet | Remove-AppxPackage
Add-AppxPackage -Register 'C:\Users\nsn\code\nickknissen\TailscaleCommandPalet\TailscaleCommandPalet\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\AppX\AppxManifest.xml'
```

Then reload the Command Palette extensions in PowerToys.
