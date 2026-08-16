# Lyrics integration

Toastify Reloaded does not scrape, store or redistribute song lyrics.

The project enables the **Lyrics Plus** Custom App bundled with Spicetify:

```powershell
spicetify config custom_apps lyrics-plus
spicetify apply
```

## First installation

Use:

```powershell
.\scripts\install-lyrics.ps1 -InstallSpicetifyIfMissing
```

The script:

1. checks for `spicetify`;
2. optionally installs Spicetify through WinGet;
3. asks to close Spotify if necessary;
4. creates/refreshes a Spotify backup;
5. enables `lyrics-plus`;
6. applies Spicetify.

## After a Spotify update

Use:

```powershell
.\scripts\restore-after-spotify-update.ps1
```

The first recovery attempt is the official standard pattern:

```powershell
spicetify backup apply
```

If that fails, the script tries a Spicetify update and then `restore backup apply`.

## Remove

```powershell
.\scripts\remove-lyrics.ps1
```

This executes:

```powershell
spicetify config custom_apps lyrics-plus-
spicetify apply
```


## Riparazione automatica dalla v1.1.0

Il Compatibility Guard rileva il cambio di versione Spotify e può eseguire automaticamente `spicetify backup apply`, mantenere `lyrics-plus` configurato e riavviare Spotify con `spicetify auto`. In caso di fallimento non entra in un ciclo infinito: la stessa versione viene ritentata automaticamente una sola volta.
