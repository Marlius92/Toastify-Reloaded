[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

function Write-Step([string]$Message) {
    Write-Host "`n==> $Message" -ForegroundColor Cyan
}

Write-Host "Toastify Modern - Ripristino Spicetify/Lyrics dopo aggiornamento Spotify" -ForegroundColor Green

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

Write-Step "Riapplicazione standard"
$firstAttemptFailed = $false
try {
    spicetify backup apply
    if ($LASTEXITCODE -ne 0) { $firstAttemptFailed = $true }
} catch {
    $firstAttemptFailed = $true
}

if ($firstAttemptFailed) {
    Write-Warning "La riapplicazione standard non è riuscita. Provo ad aggiornare Spicetify e ricostruire il backup."
    Write-Step "Aggiornamento Spicetify"
    try { spicetify update } catch { Write-Warning $_.Exception.Message }

    Write-Step "Restore + nuovo backup + apply"
    spicetify restore backup apply
}

Write-Step "Assicuro che Lyrics Plus sia ancora abilitata"
spicetify config custom_apps lyrics-plus
spicetify apply

Write-Host "`nRipristino completato. Ora riapri Spotify." -ForegroundColor Green
Read-Host "Premi Invio per chiudere"
