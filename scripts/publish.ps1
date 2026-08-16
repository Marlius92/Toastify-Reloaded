[CmdletBinding()]
param(
    [ValidateSet('win-x64','win-arm64')]
    [string]$Runtime = 'win-x64',
    [bool]$SelfContained = $true
)

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
Set-Location $repo

$outDir = Join-Path $repo "dist\ToastifyModern-$Runtime"
$zipPath = Join-Path $repo "dist\ToastifyModern-$Runtime.zip"

if (Test-Path $outDir) { Remove-Item $outDir -Recurse -Force }
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

dotnet publish .\src\ToastifyModern\ToastifyModern.csproj `
    -c Release `
    -r $Runtime `
    --self-contained $SelfContained `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugType=None `
    -o $outDir

Compress-Archive -Path "$outDir\*" -DestinationPath $zipPath -CompressionLevel Optimal
Write-Host "Creato: $zipPath" -ForegroundColor Green
