[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Write-Host "Toastify Modern - Rimozione Lyrics Plus" -ForegroundColor Yellow

if (-not (Get-Command spicetify -ErrorAction SilentlyContinue)) {
    throw "Spicetify non è installato o non è nel PATH."
}

if (Get-Process Spotify -ErrorAction SilentlyContinue) {
    Write-Warning "Chiudi Spotify prima di rimuovere Lyrics Plus."
    $answer = Read-Host "Vuoi chiuderlo automaticamente? [S/N]"
    if ($answer -match '^[sSyY]') {
        Stop-Process -Name Spotify -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
    } else {
        throw "Operazione annullata."
    }
}

spicetify config custom_apps lyrics-plus-
spicetify apply

Write-Host "Lyrics Plus rimossa dalla configurazione Spicetify." -ForegroundColor Green
Read-Host "Premi Invio per chiudere"
