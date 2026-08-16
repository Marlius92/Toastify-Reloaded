[CmdletBinding()]
param(
    [ValidateSet('win-x64','win-arm64')]
    [string]$Runtime = 'win-x64',
    [string]$MakensisPath = ''
)

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
Set-Location $repo

$projectPath = Join-Path $repo 'src\ToastifyReloaded\ToastifyReloaded.csproj'
[xml]$project = Get-Content -LiteralPath $projectPath -Raw
$version = [string]$project.Project.PropertyGroup.Version
if ([string]::IsNullOrWhiteSpace($version)) {
    throw 'Versione applicazione non trovata nel progetto.'
}

$parts = $version.Split('.')
if ($parts.Count -gt 4) { throw "Versione non valida per NSIS: $version" }
while ($parts.Count -lt 4) { $parts += '0' }
$version4 = $parts -join '.'

$distDir = Join-Path $repo 'dist'
$publishDir = Join-Path $distDir "_installer\$Runtime\app"
$setupPath = Join-Path $distDir "ToastifyReloaded-Setup-$Runtime.exe"

if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
if (Test-Path $setupPath) { Remove-Item $setupPath -Force }
New-Item -ItemType Directory -Force -Path $publishDir | Out-Null

Write-Host "Pubblicazione Toastify Reloaded $version per $Runtime..." -ForegroundColor Cyan
dotnet publish $projectPath `
    -c Release `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugType=None `
    -o $publishDir

$appExe = Join-Path $publishDir 'ToastifyReloaded.exe'
if (-not (Test-Path -LiteralPath $appExe)) {
    throw "dotnet publish non ha prodotto $appExe"
}

if ([string]::IsNullOrWhiteSpace($MakensisPath)) {
    $cmd = Get-Command makensis.exe -ErrorAction SilentlyContinue
    if ($cmd) {
        $MakensisPath = $cmd.Source
    }
    else {
        $candidates = @(
            (Join-Path ${env:ProgramFiles(x86)} 'NSIS\makensis.exe'),
            (Join-Path $env:ProgramFiles 'NSIS\makensis.exe')
        ) | Where-Object { $_ -and (Test-Path -LiteralPath $_) }
        $MakensisPath = $candidates | Select-Object -First 1
    }
}

if ([string]::IsNullOrWhiteSpace($MakensisPath) -or -not (Test-Path -LiteralPath $MakensisPath)) {
    throw 'makensis.exe non trovato. Installa NSIS oppure passa -MakensisPath.'
}

$nsiPath = Join-Path $repo 'installer\ToastifyReloaded.nsi'
$nsisArgs = @(
    "/DAPP_EXE=$appExe",
    "/DAPP_VERSION=$version",
    "/DAPP_VERSION4=$version4",
    "/DRUNTIME=$Runtime",
    "/DOUT_FILE=$setupPath",
    $nsiPath
)

Write-Host "Compilazione installer NSIS..." -ForegroundColor Cyan
& $MakensisPath @nsisArgs
if ($LASTEXITCODE -ne 0) {
    throw "makensis ha restituito exit code $LASTEXITCODE"
}

if (-not (Test-Path -LiteralPath $setupPath)) {
    throw "Installer non generato: $setupPath"
}

$stream = [System.IO.File]::OpenRead($setupPath)
try {
    if ($stream.Length -lt 2 -or $stream.ReadByte() -ne 0x4D -or $stream.ReadByte() -ne 0x5A) {
        throw 'Il file generato non ha una firma PE MZ valida.'
    }
}
finally {
    $stream.Dispose()
}

Write-Host "Installer creato: $setupPath" -ForegroundColor Green
