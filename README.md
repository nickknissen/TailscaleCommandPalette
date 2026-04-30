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

See [docs/development.md](docs/development.md).

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

See [docs/development.md](docs/development.md) for local build, signing,
and install/uninstall instructions.

## Releasing

See [docs/releasing.md](docs/releasing.md) for the GitHub Actions release
workflow and required signing secrets.

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
