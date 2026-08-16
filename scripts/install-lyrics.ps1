[CmdletBinding()]
param(
    [switch]$InstallSpicetifyIfMissing
)

$ErrorActionPreference = 'Stop'

function Write-Step([string]$Message) {
    Write-Host "`n==> $Message" -ForegroundColor Cyan
}

Write-Host "Toastify Modern - Installazione Lyrics Plus" -ForegroundColor Green
Write-Host "Questo script abilita la Custom App 'lyrics-plus' inclusa in Spicetify." 
Write-Host "Non scarica né incorpora testi musicali nel repository." 

$spicetify = Get-Command spicetify -ErrorAction SilentlyContinue
if (-not $spicetify -and $InstallSpicetifyIfMissing) {
    Write-Step "Spicetify non trovato. Installazione tramite WinGet"
    $winget = Get-Command winget -ErrorAction SilentlyContinue
    if (-not $winget) {
        throw "WinGet non è disponibile. Installa Spicetify manualmente e rilancia lo script."
    }

    winget install --id Spicetify.Spicetify --exact --accept-package-agreements --accept-source-agreements
    $env:Path = [Environment]::GetEnvironmentVariable('Path', 'Machine') + ';' + [Environment]::GetEnvironmentVariable('Path', 'User')
    $spicetify = Get-Command spicetify -ErrorAction SilentlyContinue
}

if (-not $spicetify) {
    throw "Spicetify non trovato nel PATH. Installa Spicetify e riprova."
}

if (Get-Process Spotify -ErrorAction SilentlyContinue) {
    Write-Warning "Spotify è aperto. Chiudilo completamente prima di continuare."
    $answer = Read-Host "Vuoi chiudere Spotify automaticamente? [S/N]"
    if ($answer -match '^[sSyY]') {
        Stop-Process -Name Spotify -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
    } else {
        throw "Operazione annullata: chiudi Spotify e rilancia lo script."
    }
}

Write-Step "Versione Spicetify"
spicetify --version

Write-Step "Backup di Spotify e abilitazione Lyrics Plus"
spicetify backup
spicetify config custom_apps lyrics-plus
spicetify apply

Write-Step "Verifica configurazione"
spicetify config

Write-Host "`nLyrics Plus è stata abilitata." -ForegroundColor Green
Write-Host "Apri Spotify: la voce Lyrics Plus deve comparire nella navigazione laterale." 
Write-Host "Dopo un futuro aggiornamento di Spotify usa restore-after-spotify-update.ps1." 
Read-Host "Premi Invio per chiudere"
