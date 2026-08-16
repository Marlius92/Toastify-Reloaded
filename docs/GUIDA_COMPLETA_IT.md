# Guida completa — Toastify Reloaded

## 1. Cos'è

Toastify Reloaded è un'applicazione Windows pensata per controllare Spotify senza dover portare la sua finestra in primo piano. Rimane nell'area di notifica e registra scorciatoie globali utilizzabili da qualunque programma o gioco.

È composta da due parti indipendenti:

1. **Toastify Reloaded**, l'eseguibile Windows per controlli, hotkey e popup.
2. **Lyrics Plus tramite Spicetify**, che modifica l'interfaccia locale di Spotify per aggiungere la pagina/pulsante Lyrics.

Questa separazione è importante: se Spotify si aggiorna e sostituisce i file modificati da Spicetify, le hotkey di Toastify Reloaded non dipendono da quelle modifiche.

---

## 2. Caratteristiche

### Controlli Spotify

- Play / Pausa
- Brano successivo
- Brano precedente
- Avanti di 10 secondi
- Indietro di 10 secondi

### Controlli audio Windows

- Volume +
- Volume -
- Mute

I controlli volume agiscono sul volume multimediale di Windows. I controlli di riproduzione usano invece la sessione multimediale Spotify esposta dal sistema operativo.

### Popup brano

Toastify Reloaded controlla periodicamente il brano attuale. Quando rileva un cambio, visualizza un piccolo popup in basso a destra con:

- titolo;
- artista;
- album, quando disponibile.

La durata è configurabile da 1 a 30 secondi.

### Area di notifica

Chiudendo la finestra con la X, il programma resta attivo nella tray. Dal menu dell'icona si può riaprire l'interfaccia, mostrare il popup corrente o uscire completamente.

### Avvio automatico

L'opzione “Avvia Toastify Reloaded con Windows” registra l'eseguibile nella chiave utente di avvio automatico. Non richiede privilegi amministrativi.

---

## 3. Requisiti

### Per usare una Release self-contained

- Windows 10/11
- Spotify Desktop o Spotify distribuito tramite Microsoft Store, purché esponga una sessione multimediale a Windows

### Per compilare il progetto

- Windows 10/11
- .NET 8 SDK
- Git opzionale ma consigliato

### Per Lyrics Plus

- Spotify Desktop
- Spicetify

Lo script `install-lyrics.ps1` può tentare di installare Spicetify tramite WinGet quando non è presente.

---

## 4. Prima esecuzione

1. Avvia Spotify.
2. Avvia un brano.
3. Esegui `ToastifyReloaded.exe`.
4. Nella scheda principale verifica che lo stato mostri una sessione contenente “Spotify”.
5. Prova `Ctrl+Alt+Space`.
6. Prova `Ctrl+Alt+T` per mostrare manualmente il popup.

Non viene richiesta alcuna password Spotify.

---

## 5. Hotkey

Apri la scheda **Hotkey**. Ogni scorciatoia è scritta come una sequenza separata da `+`.

Esempi validi:

```text
Ctrl+Alt+Space
Ctrl+Shift+N
Ctrl+Alt+Shift+Right
Win+Alt+P
```

Sono riconosciuti i modificatori:

- `Ctrl`
- `Alt`
- `Shift`
- `Win`

Dopo una modifica premi **Salva hotkey**. Se Windows o un altro programma ha già registrato quella combinazione, Toastify Reloaded lo segnala e lascia inattiva solo la combinazione in conflitto.

---

## 6. Aggiunta Lyrics in Spotify

La soluzione usata dal progetto è **Lyrics Plus**, Custom App inclusa in Spicetify.

### Metodo dall'interfaccia

1. Apri Toastify Reloaded.
2. Vai alla scheda **Lyrics**.
3. Premi **Installa / abilita Lyrics Plus**.
4. PowerShell si apre e verifica Spicetify.
5. Se Spotify è aperto, lo script chiede se vuoi chiuderlo.
6. Viene eseguito un backup vanilla di Spotify.
7. Viene aggiunta `lyrics-plus` alle Custom Apps.
8. Spicetify viene applicato.
9. Riapri Spotify.

### Metodo manuale

```powershell
spicetify backup
spicetify config custom_apps lyrics-plus
spicetify apply
```

Lyrics Plus compare nell'interfaccia di Spotify come applicazione di navigazione.

---

## 7. Quando Spotify si aggiorna e Lyrics sparisce

Spotify può sostituire i file modificati da Spicetify durante un aggiornamento. In questo caso **non è necessario reinstallare Toastify Reloaded**.

Apri la scheda **Lyrics** e premi:

**Ripristina Lyrics dopo un aggiornamento Spotify**

Oppure esegui:

```powershell
.\scripts\restore-after-spotify-update.ps1
```

Lo script prova prima:

```powershell
spicetify backup apply
```

Se la prima riapplicazione fallisce, tenta anche l'aggiornamento di Spicetify e una ricostruzione del backup.

---

## 8. Rimozione Lyrics

Dall'interfaccia premi **Rimuovi Lyrics Plus**, oppure:

```powershell
.\scripts\remove-lyrics.ps1
```

Il comando Spicetify usato è:

```powershell
spicetify config custom_apps lyrics-plus-
spicetify apply
```

---

## 9. Come funziona tecnicamente

### Sessione Spotify

`SpotifySessionService` richiede a Windows il `GlobalSystemMediaTransportControlsSessionManager`, cerca una sessione il cui `SourceAppUserModelId` contenga la parola `spotify` e usa quella sessione per leggere i metadati e inviare comandi.

Questo evita di:

- cercare la finestra principale di Spotify;
- simulare click sull'interfaccia;
- salvare credenziali;
- usare un client secret della Spotify Web API.

### Hotkey globali

`GlobalHotkeyService` usa la funzione Win32 `RegisterHotKey`. Windows invia quindi `WM_HOTKEY` alla finestra dell'applicazione anche quando l'app non è in primo piano.

### Seek

Il salto di ±10 secondi legge la posizione corrente della sessione multimediale e chiama `TryChangePlaybackPositionAsync` con la nuova posizione.

### Volume

Il volume viene modificato simulando i tasti multimediali standard di Windows tramite `SendInput`.

### Configurazione

Le preferenze sono salvate in:

```text
%APPDATA%\ToastifyReloaded\settings.json
```

Il file può essere cancellato per tornare ai valori predefiniti.

---

## 10. Compilazione

Apri PowerShell nella cartella del repository:

```powershell
.\scripts\build.ps1
```

Equivalente manuale:

```powershell
dotnet restore .\ToastifyReloaded.sln
dotnet build .\ToastifyReloaded.sln -c Release --no-restore
```

---

## 11. Creazione ZIP distribuibile

Per Windows x64:

```powershell
.\scripts\publish.ps1 -Runtime win-x64 -SelfContained $true
```

Per Windows ARM64:

```powershell
.\scripts\publish.ps1 -Runtime win-arm64 -SelfContained $true
```

La build self-contained include il runtime .NET necessario e genera un archivio nella cartella `dist`.

---

## 12. Pubblicazione su GitHub

Dopo aver creato un repository vuoto su GitHub:

```powershell
git init
git add .
git commit -m "Initial release"
git branch -M main
git remote add origin https://github.com/Marlius92/Toastify-Reloaded.git
git push -u origin main
```

Il workflow `build.yml` viene eseguito automaticamente a ogni push o pull request su `main`.

Per creare una Release automatica:

```powershell
git tag v1.0.0
git push origin v1.0.0
```

Il workflow `release.yml` crea i pacchetti x64 e ARM64 e li allega alla Release.

---

## 13. Diagnostica

Esegui:

```powershell
.\scripts\diagnose.ps1
```

Mostra:

- versione Windows;
- versione PowerShell;
- processo Spotify;
- versione Spicetify;
- percorso della configurazione Spicetify;
- configurazione Spicetify;
- configurazione Toastify Reloaded.

Prima di pubblicare l'output in una issue GitHub, controlla se contiene percorsi personali che preferisci rimuovere.

---

## 14. Problemi comuni

### “Spotify non rilevato”

Avvia un brano, premi **Aggiorna** e verifica che Windows mostri Spotify nei controlli multimediali. Se il problema continua, usa la diagnostica.

### “Scorciatoia già usata”

Scegli una combinazione diversa: `RegisterHotKey` non può sottrarre in modo affidabile una hotkey già registrata da un'altra applicazione.

### Lyrics sparita dopo un update

Esegui il ripristino Spicetify. Non reinstallare Toastify Reloaded.

### Lyrics Plus apre la pagina ma non mostra testo

L'applicazione Toastify Reloaded non gestisce i provider Lyrics. Apri le impostazioni di Lyrics Plus dentro Spotify e verifica il provider disponibile.
