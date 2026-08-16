[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

function Write-Step([string]$Message) {
    Write-Host "`n==> $Message" -ForegroundColor Cyan
}

Write-Host "Toastify Reloaded - Ripristino Spicetify/Lyrics dopo aggiornamento Spotify" -ForegroundColor Green

if (-not (Get-Command spicetify -ErrorAction SilentlyContinue)) {
    throw "Spicetify non è installato o non è nel PATH."
}

if (Get-Process Spotify -ErrorAction SilentlyContinue) {
    Write-Warning "Spotify è aperto."
    $answer = Read-Host "Vuoi chiuderlo automaticamente? [S/N]"
    if ($answer -match '^[sSyY]') {
        Stop-Process -Name Spotify -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
    } else {
        throw "Operazione annullata: chiudi Spotify e rilancia lo script."
    }
}

Write-Step "Aggiornamento opzionale di Spicetify"
try { spicetify upgrade } catch { Write-Warning $_.Exception.Message }

Write-Step "Riapplicazione raccomandata dopo update Spotify"
$firstAttemptFailed = $false
try {
    spicetify backup apply
    if ($LASTEXITCODE -ne 0) { $firstAttemptFailed = $true }
} catch {
    $firstAttemptFailed = $true
}

if ($firstAttemptFailed) {
    Write-Warning "La riapplicazione standard non è riuscita. Provo una ricostruzione completa."
    Write-Step "Restore + nuovo backup + apply"
    spicetify restore backup apply
    if ($LASTEXITCODE -ne 0) { throw "Spicetify non è ancora compatibile con questa versione di Spotify oppure la riparazione è fallita." }
}

Write-Step "Assicuro che Lyrics Plus sia ancora abilitata"
$currentApps = spicetify config custom_apps | Out-String
if ($currentApps -notmatch 'lyrics-plus') {
    spicetify config custom_apps lyrics-plus
}
spicetify apply

Write-Step "Avvio Spotify tramite Spicetify Auto"
spicetify auto

Write-Host "`nRipristino completato." -ForegroundColor Green
Read-Host "Premi Invio per chiudere"
