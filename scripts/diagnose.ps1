[CmdletBinding()]
param()

$ErrorActionPreference = 'Continue'
Write-Host "Toastify Modern - Diagnostica" -ForegroundColor Green
Write-Host "Data: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')"
Write-Host "Windows: $([Environment]::OSVersion.VersionString)"
Write-Host "PowerShell: $($PSVersionTable.PSVersion)"
Write-Host "Architettura OS: $([Runtime.InteropServices.RuntimeInformation]::OSArchitecture)"
Write-Host "Architettura processo: $([Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture)"

Write-Host "`n--- Spotify ---" -ForegroundColor Cyan
$spotify = Get-Process Spotify -ErrorAction SilentlyContinue
if ($spotify) {
    $spotify | Select-Object Id, ProcessName, Path | Format-Table -AutoSize
} else {
    Write-Host "Spotify non è in esecuzione."
}

Write-Host "`n--- Spicetify ---" -ForegroundColor Cyan
if (Get-Command spicetify -ErrorAction SilentlyContinue) {
    spicetify --version
    Write-Host "Config file:"
    spicetify -c
    Write-Host "`nConfigurazione corrente:"
    spicetify config
} else {
    Write-Host "Spicetify non trovato nel PATH."
}

Write-Host "`n--- Toastify Modern ---" -ForegroundColor Cyan
$config = Join-Path $env:APPDATA 'ToastifyModern\settings.json'
Write-Host "Config: $config"
if (Test-Path $config) {
    Get-Content $config
} else {
    Write-Host "settings.json non ancora creato."
}

Write-Host "`nCopia questo output in una issue GitHub, eliminando eventuali percorsi personali se non vuoi pubblicarli." -ForegroundColor Yellow
Read-Host "Premi Invio per chiudere"
