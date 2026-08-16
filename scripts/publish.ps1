[CmdletBinding()]
param(
    [ValidateSet('win-x64','win-arm64')]
    [string]$Runtime = 'win-x64',
    [bool]$SelfContained = $true
)

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
Set-Location $repo

$distDir = Join-Path $repo 'dist'
$outDir = Join-Path $distDir "_publish\$Runtime"
$zipPath = Join-Path $distDir "ToastifyReloaded-$Runtime.zip"

if (Test-Path $outDir) { Remove-Item $outDir -Recurse -Force }
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

dotnet publish .\src\ToastifyReloaded\ToastifyReloaded.csproj `
    -c Release `
    -r $Runtime `
    --self-contained $SelfContained `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugType=None `
    -o $outDir

$exePath = Join-Path $outDir 'ToastifyReloaded.exe'
if (-not (Test-Path -LiteralPath $exePath)) {
    throw "La pubblicazione non ha prodotto ToastifyReloaded.exe in $outDir"
}

# La Release pubblica deve essere pulita: il pacchetto contiene soltanto
# l'eseguibile single-file. Gli script di manutenzione sono EmbeddedResource
# dentro l'assembly e restano disponibili dalla UI.
Compress-Archive -LiteralPath $exePath -DestinationPath $zipPath -CompressionLevel Optimal

# Gate semplice: impedisce di pubblicare accidentalmente cartelle scripts o
# altri file in futuro.
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    $entries = @($archive.Entries | Where-Object { -not [string]::IsNullOrWhiteSpace($_.Name) })
    if ($entries.Count -ne 1 -or $entries[0].FullName -ne 'ToastifyReloaded.exe') {
        $names = ($entries | ForEach-Object FullName) -join ', '
        throw "Pacchetto Release non valido. Contenuto trovato: $names"
    }
}
finally {
    $archive.Dispose()
}

Write-Host "Creato pacchetto single-EXE: $zipPath" -ForegroundColor Green
