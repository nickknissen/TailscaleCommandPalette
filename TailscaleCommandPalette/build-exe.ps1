param(
    [string]$ExtensionName = "TailscaleCommandPalette",
    [string]$Configuration = "Release",
    [string]$Version = "2.0.0",
    [string[]]$Platforms = @("x64", "arm64"),
    [string]$SourceRevisionId = $env:GITHUB_SHA
)

$ErrorActionPreference = "Stop"

Write-Host "Building $ExtensionName EXE installer..." -ForegroundColor Green
Write-Host "Version: $Version" -ForegroundColor Yellow
Write-Host "Platforms: $($Platforms -join ', ')" -ForegroundColor Yellow
if (-not [string]::IsNullOrWhiteSpace($SourceRevisionId)) {
    Write-Host "Source revision: $SourceRevisionId" -ForegroundColor Yellow
}

$ProjectDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectFile = Join-Path $ProjectDir "$ExtensionName.csproj"
$SetupTemplatePath = Join-Path $ProjectDir "setup-template.iss"

if (-not (Test-Path $ProjectFile)) {
    throw "Project file not found: $ProjectFile"
}

if (-not (Test-Path $SetupTemplatePath)) {
    throw "Setup template not found: $SetupTemplatePath"
}

Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
foreach ($path in @("bin", "obj")) {
    $fullPath = Join-Path $ProjectDir $path
    if (Test-Path $fullPath) {
        Remove-Item -Path $fullPath -Recurse -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
& dotnet restore $ProjectFile
if ($LASTEXITCODE -ne 0) {
    throw "dotnet restore failed with exit code $LASTEXITCODE"
}

$InnoSetupCandidates = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\iscc.exe",
    "${env:ProgramFiles}\Inno Setup 6\iscc.exe"
)
$InnoSetupPath = $InnoSetupCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $InnoSetupPath) {
    throw "Inno Setup not found. Install Inno Setup 6 first."
}

foreach ($Platform in $Platforms) {
    Write-Host "`n=== Building $Platform ===" -ForegroundColor Cyan

    $publishDir = Join-Path $ProjectDir "bin\$Configuration\win-$Platform\publish"

    Write-Host "Publishing win-$Platform..." -ForegroundColor Yellow
    $publishArgs = @(
        'publish',
        $ProjectFile,
        '--configuration', $Configuration,
        '--runtime', "win-$Platform",
        '--self-contained', 'true',
        '--output', $publishDir,
        "/p:Version=$Version",
        "/p:AssemblyVersion=$Version.0",
        "/p:FileVersion=$Version.0",
        '/p:WindowsPackageType=None'
    )

    if (-not [string]::IsNullOrWhiteSpace($SourceRevisionId)) {
        $publishArgs += "/p:SourceRevisionId=$SourceRevisionId"
    }

    & dotnet @publishArgs

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed for $Platform with exit code $LASTEXITCODE"
    }

    $fileCount = (Get-ChildItem -Path $publishDir -Recurse -File | Measure-Object).Count
    Write-Host "Published $fileCount files to $publishDir" -ForegroundColor Green

    Write-Host "Creating installer script for $Platform..." -ForegroundColor Yellow
    $setupTemplate = Get-Content $SetupTemplatePath -Raw
    $setupScript = $setupTemplate -replace '#define AppVersion ".*"', "#define AppVersion `"$Version`""
    $setupScript = $setupScript -replace 'OutputBaseFilename=(.*?)\{#AppVersion\}', "OutputBaseFilename=`$1{#AppVersion}-$Platform"
    $setupScript = $setupScript -replace 'Source: "bin\\Release\\win-x64\\publish\\\*"', "Source: `"bin\$Configuration\win-$Platform\publish\*`""

    if ($Platform -eq "arm64") {
        $setupScript = $setupScript -replace '(MinVersion=10\.0\.19041)', "ArchitecturesAllowed=arm64`r`nArchitecturesInstallIn64BitMode=arm64`r`n`$1"
    }
    else {
        $setupScript = $setupScript -replace '(MinVersion=10\.0\.19041)', "ArchitecturesAllowed=x64compatible`r`nArchitecturesInstallIn64BitMode=x64compatible`r`n`$1"
    }

    $setupScriptPath = Join-Path $ProjectDir "setup-$Platform.iss"
    Set-Content -Path $setupScriptPath -Value $setupScript -Encoding utf8 -NoNewline

    Write-Host "Creating $Platform installer with Inno Setup..." -ForegroundColor Yellow
    & $InnoSetupPath $setupScriptPath
    if ($LASTEXITCODE -ne 0) {
        throw "Inno Setup failed for $Platform with exit code $LASTEXITCODE"
    }

    $installer = Get-ChildItem (Join-Path $ProjectDir "bin\$Configuration\installer\*-$Platform.exe") -ErrorAction Stop | Select-Object -First 1
    $sizeMB = [math]::Round($installer.Length / 1MB, 2)
    Write-Host "Created $Platform installer: $($installer.Name) ($sizeMB MB)" -ForegroundColor Green
}

Write-Host "`nBuild completed successfully." -ForegroundColor Green
Get-ChildItem (Join-Path $ProjectDir "bin\$Configuration\installer") -File | Select-Object Name, Length
