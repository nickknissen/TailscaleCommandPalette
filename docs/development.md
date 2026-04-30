# Development

## Local build for testing

```powershell
.\TailscaleCommandPalette\build-msix.ps1 -Version 2.0.4 -Platforms @('x64','arm64') -Bundle
```

Produces `TailscaleCommandPalette\bin\Release\msix\TailscaleCommandPalette_2.0.4.0.msixbundle`.
With no `-CertPath` / `-CertBase64`, the package is left unsigned.

For a single-architecture build during development:

```powershell
.\TailscaleCommandPalette\build-msix.ps1 -Version 2.0.4 -Platforms x64
```

## Sign locally

The shared CmdPal signing cert lives in 1Password under
`Private/CmdPal Signing Cert`. The same cert is used by
[TablePlusCommandPalette](https://github.com/nickknissen/TablePlusCommandPalette),
[TailscaleCommandPalette](https://github.com/nickknissen/TailscaleCommandPalette),
and
[SSMSCommandPalette](https://github.com/nickknissen/SSMSCommandPalette).

```powershell
.\scripts\sign-local.ps1 -Path .\TailscaleCommandPalette\bin\Release\msix\*.msix*
```

## Install / uninstall

```powershell
Add-AppxPackage .\TailscaleCommandPalette\bin\Release\msix\TailscaleCommandPalette_2.0.4.0_x64.msix

# Remove every installed copy (sideloaded, dev-registered):
.\scripts\uninstall.ps1
```
