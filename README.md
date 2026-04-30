# Tailscale Command Palette

A PowerToys Command Palette extension that brings common Tailscale actions and device information into the Command Palette UI.

## Screenshot

![Tailscale Command Palette in PowerToys Command Palette](docs/assets/powertoys-command-palette-screenshot.png)

## Features

- Browse **all devices** in your tailnet
- Browse **only your devices**
- View **connection status** and local Tailscale details
- Quickly **connect** or **disconnect** Tailscale
- Open the **Tailscale Admin Console**
- Copy useful device data:
  - Tailscale IPv4
  - Tailscale IPv6
  - MagicDNS name
- See device metadata at a glance:
  - online/offline state
  - operating system
  - current device
  - exit node
  - SSH availability

## How it works

This extension shells out to the local `tailscale` CLI and parses `tailscale status --json` to populate Command Palette pages and actions.

## Requirements

- **Windows 10 2004+** or **Windows 11**
- **.NET 9 SDK** for local development
- **Tailscale** installed and signed in
- The `tailscale` command available on your `PATH`
- Microsoft PowerToys with **Command Palette** support

## Installation

### Option 1: WinGet

Install from WinGet:

```powershell
winget install nickknissen.TailscaleCommandPalette
```

### Option 2: Manual install

1. Download the latest installer for your architecture from the project releases.
2. Run the installer.
3. Restart Command Palette if it is already running.

## Usage

After installation, open **PowerToys Command Palette** and look for the **Tailscale** provider.

Top-level commands include:

- **All Devices**
- **My Devices**
- **Status**
- **Connection**
- **Admin Console**

### Device actions

Selecting a device copies its primary Tailscale IP by default.

Additional context actions may include:

- **Copy IPv6**
- **Copy MagicDNS**
- **Open in browser**

## Screens / Commands

### All Devices

Shows the full visible tailnet inventory, grouped by:

- **This Device**
- **Online**
- **Offline**

### My Devices

Filters the device list to the currently signed-in Tailscale user.

### Status

Shows:

- connection state
- hostname
- local Tailscale IPs
- tailnet name
- active exit node
- visible device count
- extension version and commit metadata

### Connection

Changes automatically based on the current state:

- **Up** when disconnected
- **Down** when connected

## Development

### Build from source

```powershell
dotnet restore
dotnet build TailscaleCommandPalette.sln
```

### Publish a self-contained build

Example for x64:

```powershell
dotnet publish .\TailscaleCommandPalette\TailscaleCommandPalette.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  /p:WindowsPackageType=None
```

## Releasing

A new release is cut by triggering the `Release Extension` GitHub Actions
workflow. It builds signed x64 + ARM64 MSIX via `build-msix.ps1`,
combines them into a single `.msixbundle`, and creates a GitHub Release
with the bundle and individual MSIX files attached.

```powershell
gh workflow run release.yml --repo nickknissen/TailscaleCommandPalette `
  -f version=2.0.4 `
  -f release_notes="One-line summary of what changed in this release."
```

When the run finishes:

1. The release appears at
   `https://github.com/nickknissen/TailscaleCommandPalette/releases/tag/<version>`.
2. The `update-winget.yml` workflow fires automatically and submits a
   `wingetcreate` PR to `microsoft/winget-pkgs`.
3. To also push the same artifact to the Microsoft Store: download
   `TailscaleCommandPalette_<version>.0.msixbundle` from the release page
   (or `gh release download <version> --pattern *.msixbundle`), then upload
   it in [Partner Center](https://partner.microsoft.com/dashboard/home)
   under your app's **Packages** section. The Store re-signs the package
   during ingestion regardless of the build-time signature.

### Signing prerequisites

The release workflow expects two GitHub repository secrets:

- `SIGNING_PFX_BASE64` — base64-encoded PFX containing the code-signing
  certificate. The cert subject must match the `Publisher` declared in
  `Package.appxmanifest`.
- `SIGNING_PFX_PASSWORD` — PFX password.

The shared CmdPal signing cert (used by this repo and
[TablePlusCommandPalette](https://github.com/nickknissen/TablePlusCommandPalette))
lives in 1Password under `Private/CmdPal Signing Cert`.

### Local build for testing

```powershell
.\TailscaleCommandPalette\build-msix.ps1 -Version 2.0.4 -Platforms @('x64','arm64') -Bundle
```

Produces `bin/Release/msix/TailscaleCommandPalette_2.0.4.0.msixbundle`. With
no `-CertPath`/`-CertPassword`, the package is left unsigned.

To sign locally with the cert from 1Password:

```powershell
.\scripts\sign-local.ps1 -Path .\TailscaleCommandPalette\bin\Release\msix\*.msix*
```

## Project structure

```text
TailscaleCommandPalette/
├─ Commands/      # Command Palette invokable commands
├─ Models/        # Tailscale status/device models
├─ Pages/         # Command Palette list pages
├─ Services/      # Tailscale CLI integration and parsing
├─ Assets/        # App and extension icons
└─ build-msix.ps1 # Signed MSIX build script (used by release.yml)
```

## Troubleshooting

### The extension shows an error instead of devices

Common causes:

- Tailscale is not installed
- Tailscale is not running
- The device is not connected to a tailnet
- `tailscale.exe` is not on `PATH`

### Connection actions do not work

Make sure the Tailscale CLI works outside the extension first:

```powershell
tailscale status
tailscale up
tailscale down
```

### Device list is stale

The extension caches CLI results briefly to avoid excessive shelling out. Reopen the page or retry after a few seconds.

## License

This project is licensed under the [MIT License](LICENSE).

## Disclaimer

This project is an independent extension for Microsoft PowerToys and is not affiliated with or endorsed by Tailscale or Microsoft.
