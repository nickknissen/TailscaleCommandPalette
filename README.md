# Tailscale Command Palette

A PowerToys Command Palette extension that brings common Tailscale actions
and device information into the Command Palette UI.

![Screenshot](docs/assets/powertoys-command-palette-screenshot.jpg)

## Features

- Browse **all devices** in your tailnet
- Browse **only your devices**
- View **connection status** and local Tailscale details
- Quickly **connect** or **disconnect** Tailscale
- Open the **Tailscale Admin Console**
- Copy useful device data: Tailscale IPv4, Tailscale IPv6, MagicDNS name
- See device metadata at a glance: online/offline state, OS, current device,
  exit node, SSH availability

## How it works

The extension shells out to the local `tailscale` CLI and parses
`tailscale status --json` to populate Command Palette pages and actions.

## Requirements

- **Windows 10 2004+** or **Windows 11**
- **Tailscale** installed and signed in, with `tailscale.exe` on `PATH`
- Microsoft PowerToys with **Command Palette** support

## Installation

### Microsoft Store

[![Get from Microsoft Store](https://img.shields.io/badge/Microsoft%20Store-Get-blue?logo=microsoft-store)](https://apps.microsoft.com/detail/9N5NWRQ9FBLM)

Open the listing: <https://apps.microsoft.com/detail/9N5NWRQ9FBLM>

### WinGet

```powershell
winget install nickknissen.TailscaleCommandPalette
```

### Sideload signed MSIX

Download the latest `.msixbundle` from the
[Releases page](https://github.com/nickknissen/TailscaleCommandPalette/releases),
then:

```powershell
Add-AppxPackage .\TailscaleCommandPalette_<version>.msixbundle
```

### Build from source

See [Development](#development) below.

## Usage

After installation, open **PowerToys Command Palette** and look for the
**Tailscale** provider.

Top-level commands:

- **All Devices** — full visible tailnet inventory, grouped by *This Device*
  / *Online* / *Offline*
- **My Devices** — devices owned by the currently signed-in Tailscale user
- **Status** — connection state, hostname, local Tailscale IPs, tailnet
  name, active exit node, visible device count
- **Connection** — toggle between *Up* (when disconnected) and *Down* (when
  connected)
- **Admin Console** — opens <https://login.tailscale.com/admin/machines>

Selecting a device copies its primary Tailscale IP by default. Additional
context actions include **Copy IPv6**, **Copy MagicDNS**, and **Open in
browser**.

## Development

### Local build for testing

```powershell
.\TailscaleCommandPalette\build-msix.ps1 -Version 2.0.4 -Platforms @('x64','arm64') -Bundle
```

Produces `TailscaleCommandPalette\bin\Release\msix\TailscaleCommandPalette_2.0.4.0.msixbundle`.
With no `-CertPath` / `-CertBase64`, the package is left unsigned.

For a single-architecture build during development:

```powershell
.\TailscaleCommandPalette\build-msix.ps1 -Version 2.0.4 -Platforms x64
```

### Sign locally

The shared CmdPal signing cert lives in 1Password under
`Private/CmdPal Signing Cert`. The same cert is used by
[TablePlusCommandPalette](https://github.com/nickknissen/TablePlusCommandPalette),
[TailscaleCommandPalette](https://github.com/nickknissen/TailscaleCommandPalette),
and
[SSMSCommandPalette](https://github.com/nickknissen/SSMSCommandPalette).

```powershell
.\scripts\sign-local.ps1 -Path .\TailscaleCommandPalette\bin\Release\msix\*.msix*
```

### Install / uninstall

```powershell
Add-AppxPackage .\TailscaleCommandPalette\bin\Release\msix\TailscaleCommandPalette_2.0.4.0_x64.msix

# Remove every installed copy (sideloaded, dev-registered):
.\scripts\uninstall.ps1
```

## Releasing

A new release is cut by triggering the `Release Extension` GitHub Actions
workflow. It builds signed x64 + ARM64 MSIX via `build-msix.ps1`, combines
them into a single `.msixbundle`, and creates a GitHub Release with the
bundle and individual MSIX files attached.

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

The release workflow expects two GitHub repository secrets:

- `SIGNING_PFX_BASE64` — base64-encoded PFX containing the code-signing
  certificate. The cert subject must match the `Publisher` declared in
  `Package.appxmanifest`.
- `SIGNING_PFX_PASSWORD` — PFX password.

## Project structure

```text
TailscaleCommandPalette/
├─ Commands/      # Command Palette invokable commands
├─ Models/        # Tailscale status / device models
├─ Pages/         # Command Palette list pages
├─ Services/      # Tailscale CLI integration and parsing
├─ Assets/        # App and extension icons
└─ build-msix.ps1 # Signed MSIX build script (used by release.yml)
```

## Troubleshooting

### The extension shows an error instead of devices

Common causes: Tailscale isn't installed, isn't running, the device isn't
connected to a tailnet, or `tailscale.exe` isn't on `PATH`.

### Connection actions do not work

Make sure the Tailscale CLI works outside the extension first:

```powershell
tailscale status
tailscale up
tailscale down
```

### Device list is stale

The extension caches CLI results briefly to avoid excessive shelling out.
Reopen the page or retry after a few seconds.

## License

This project is licensed under the [MIT License](LICENSE).

## Disclaimer

This project is an independent extension for Microsoft PowerToys and is
not affiliated with or endorsed by Tailscale or Microsoft.
