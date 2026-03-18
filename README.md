# Tailscale Command Palette

A [PowerToys Command Palette](https://learn.microsoft.com/en-us/windows/powertoys/command-palette) extension that lets you quickly find and interact with your Tailscale devices.

![PowerToys Command Palette](https://img.shields.io/badge/PowerToys-Command%20Palette-blue)
![Platform](https://img.shields.io/badge/platform-Windows%2010%2B-lightgrey)

## Features

- List all devices on your Tailscale network, grouped by status (This Device, Online, Offline)
- **Copy IP address** to clipboard (default action)
- **Copy DNS name** to clipboard (Shift)
- **Open in browser** via DNS name (Ctrl)
- Displays device OS, online/offline status, exit node, and self indicators as color-coded tags
- 30-second device cache for fast repeated lookups

## Prerequisites

- Windows 10 or later (x64 or ARM64)
- [PowerToys](https://github.com/microsoft/PowerToys) with Command Palette enabled
- [Tailscale](https://tailscale.com/) installed and available on your PATH

## Installation

### Build from source

```sh
cd TailscaleCommandPalet

dotnet publish TailscaleCommandPalet.csproj -c Release -p:Platform=x64 -p:RuntimeIdentifier=win-x64
```

For ARM64, replace `x64` / `win-x64` with `ARM64` / `win-arm64`.

### Register the extension

Copy the published output into the AppX folder, then register:

```powershell
# Copy build output
Copy-Item bin\Release\net10.0-windows10.0.26100.0\win-x64\publish\TailscaleCommandPalet.dll `
  bin\x64\Release\net10.0-windows10.0.26100.0\win-x64\AppX\TailscaleCommandPalet.dll
Copy-Item bin\Release\net10.0-windows10.0.26100.0\win-x64\publish\TailscaleCommandPalet.exe `
  bin\x64\Release\net10.0-windows10.0.26100.0\win-x64\AppX\TailscaleCommandPalet.exe

# Register (re-register if already installed)
Get-AppxPackage -Name TailscaleCommandPalet | Remove-AppxPackage
Add-AppxPackage -Register "$PWD\bin\x64\Release\net10.0-windows10.0.26100.0\win-x64\AppX\AppxManifest.xml"
```

Then reload extensions in the PowerToys Command Palette settings.

## Usage

1. Open PowerToys Command Palette
2. Select the **Tailscale Devices** command
3. Browse your devices — they're grouped into **This Device**, **Online**, and **Offline** sections
4. Press **Enter** to copy the device's Tailscale IP, **Shift+Enter** to copy the DNS name, or **Ctrl+Enter** to open it in a browser

## License

MIT
