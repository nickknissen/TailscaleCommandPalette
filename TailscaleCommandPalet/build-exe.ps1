param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [string[]]$Platforms = @("x64", "arm64")
)

$ErrorActionPreference = "Stop"
$ProjectDir = $PSScriptRoot
$CsprojPath = Join-Path $ProjectDir "TailscaleCommandPalet.csproj"
$TemplateFile = Join-Path $ProjectDir "setup-template.iss"
$InstallerDir = Join-Path $ProjectDir "bin\Release\installer"

if (-not (Test-Path $TemplateFile)) {
    Write-Error "Inno Setup template not found: $TemplateFile"
    exit 1
}

if (-not (Test-Path $InstallerDir)) {
    New-Item -ItemType Directory -Path $InstallerDir -Force | Out-Null
}

foreach ($platform in $Platforms) {
    Write-Host "=== Building $platform ===" -ForegroundColor Cyan

    dotnet publish $CsprojPath `
        -c Release `
        -p:Platform=$platform `
        -p:RuntimeIdentifier=win-$platform `
        --self-contained true

    if ($LASTEXITCODE -ne 0) {
        Write-Error "dotnet publish failed for $platform"
        exit 1
    }

    Write-Host "=== Creating installer for $platform ===" -ForegroundColor Cyan

    # Find the publish directory (dotnet publish with -p:Platform outputs to bin\<platform>\Release\...)
    $publishDir = Get-ChildItem -Path (Join-Path $ProjectDir "bin") -Recurse -Directory -Filter "publish" |
        Where-Object { $_.FullName -match "Release" -and $_.FullName -match "win-$platform" } |
        Select-Object -First 1 -ExpandProperty FullName

    if (-not $publishDir) {
        Write-Error "Could not find publish directory for $platform"
        exit 1
    }

    Write-Host "Publish directory: $publishDir"

    $issContent = Get-Content $TemplateFile -Raw
    $issContent = $issContent.Replace('{{VERSION}}', $Version)
    $issContent = $issContent.Replace('{{PLATFORM}}', $platform)
    $issContent = $issContent.Replace('{{PUBLISH_DIR}}', $publishDir)
    $issContent = $issContent.Replace('{{OUTPUT_DIR}}', $InstallerDir)

    $issFile = Join-Path $ProjectDir "setup-$platform.iss"
    $issContent | Set-Content -Path $issFile -Encoding UTF8

    iscc $issFile

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Inno Setup compilation failed for $platform"
        exit 1
    }

    Remove-Item $issFile -Force
    Write-Host "=== $platform installer created ===" -ForegroundColor Green
}

Write-Host ""
Write-Host "Installers available in: $InstallerDir" -ForegroundColor Green
Get-ChildItem $InstallerDir -Filter "*.exe" | ForEach-Object { Write-Host "  $_" }
