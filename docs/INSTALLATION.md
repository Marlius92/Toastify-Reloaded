# Installazione di Toastify Reloaded

Dalla versione 1.2.0 Toastify Reloaded viene distribuito come vera applicazione Windows, non come pacchetto portable.

## Installer disponibili

- `ToastifyReloaded-Setup-win-x64.exe` — PC Windows Intel/AMD a 64 bit.
- `ToastifyReloaded-Setup-win-arm64.exe` — PC Windows ARM64.

## Cosa fa il Setup

L'installer usa NSIS, come lo storico installer di Toastify, e richiede l'autorizzazione UAC per installare l'applicazione per il computer.

Percorso predefinito:

```text
C:\Program Files\Toastify Reloaded\
```

Vengono creati:

- `ToastifyReloaded.exe`;
- `Uninstall.exe`;
- collegamento **Toastify Reloaded** nel menu Start;
- collegamento **Disinstalla Toastify Reloaded** nel menu Start;
- voce **Toastify Reloaded** in Impostazioni → App → App installate.

Gli helper Lyrics/diagnostica sono incorporati nell'EXE e non vengono installati come cartella `scripts`.

## Dati utente

Le impostazioni continuano a essere salvate in:

```text
%APPDATA%\ToastifyReloaded\settings.json
```

L'aggiornamento e la disinstallazione non cancellano automaticamente questo file, così hotkey, popup e impostazioni di Compatibility Guard possono essere mantenuti tra reinstallazioni.

## Aggiornamenti automatici

Toastify Reloaded cerca il Setup corretto per l'architettura del PC nella Latest Release GitHub. Quando è disponibile una versione più recente:

1. scarica il nuovo Setup in una cartella temporanea;
2. Windows mostra il normale controllo UAC;
3. dopo l'approvazione, il Setup attende la chiusura della versione corrente;
4. aggiorna i file in Program Files;
5. riavvia Toastify Reloaded.

L'aggiornamento non modifica `%APPDATA%\ToastifyReloaded`.

## Passaggio dalle versioni portable 1.1.x

Il passaggio iniziale dalla serie portable 1.1.x alla serie installabile 1.2.x va eseguito una volta avviando manualmente il nuovo `ToastifyReloaded-Setup-*.exe`. Dopo la 1.2.0 gli aggiornamenti successivi usano direttamente il nuovo installer.

Il vecchio EXE portable non viene eliminato automaticamente: dopo aver verificato che la versione installata funzioni, può essere cancellato manualmente.
