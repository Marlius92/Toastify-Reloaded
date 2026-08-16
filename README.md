# Toastify Reloaded

**Toastify Reloaded** is a Windows companion for Spotify with global hotkeys, track popups, Spicetify/Lyrics integration and an automatic Compatibility Guard designed to recover after Spotify updates.

> Independent community project. Not affiliated with Spotify AB. Toastify Reloaded is a modern reimplementation inspired by the discontinued Toastify utility.

## Main features

- Global hotkeys even while Spotify is in the background.
- Play/Pause, Next and Previous track.
- Seek forward/backward by 10 seconds.
- Windows volume up/down and mute.
- Customizable shortcuts.
- Popup when the current track changes.
- System-tray operation and optional Windows startup.
- No Spotify username/password or developer API key required.
- One-click **Lyrics Plus** integration through Spicetify.
- **Compatibility Guard** that records the installed Spotify version and detects future upgrades.
- Automatic `spicetify backup apply` after a detected Spotify upgrade.
- Automatic fallback to `spicetify restore backup apply` if the standard recovery fails.
- Optional Spicetify CLI upgrade before recovery.
- Optional automatic re-enable of Lyrics Plus.
- Automatic Spotify restart through `spicetify auto` after recovery.
- Repair-loop protection: a failed automatic repair is not repeated endlessly for the same Spotify version.
- Automatic Toastify Reloaded updates from this repository's GitHub Releases.
- Architecture-aware update downloads for Windows x64 and ARM64.
- Local JSON configuration in `%APPDATA%\ToastifyReloaded\settings.json`.

## Compatibility Guard (v1.1.0)

Toastify Reloaded keeps the media-control core separate from Spotify's user interface. Playback controls use Windows media sessions, while Lyrics customization is handled through Spicetify.

At startup and periodically while the app is running, the Compatibility Guard checks the installed Spotify version. The first detected version is stored as a baseline. When a later Spotify build is detected, Toastify Reloaded can automatically:

1. close Spotify;
2. optionally run `spicetify upgrade`;
3. run the official post-update recovery command `spicetify backup apply`;
4. fall back to `spicetify restore backup apply` if needed;
5. ensure `lyrics-plus` remains configured;
6. apply the customization;
7. run `spicetify auto` to launch Spotify again;
8. record the new Spotify version only after a successful repair.

If Spicetify does not yet support a brand-new Spotify release, Toastify Reloaded reports the failure and avoids an automatic retry loop for that same version. The user can force another attempt later from **Aggiornamenti → Ripara Spotify / Lyrics ora**.

## Automatic Toastify Reloaded updates

When enabled, Toastify Reloaded checks the public GitHub Latest Release endpoint for `Marlius92/Toastify-Reloaded`. If a newer version exists, it selects the matching asset:

- `ToastifyReloaded-win-x64.zip`
- `ToastifyReloaded-win-arm64.zip`

The update is downloaded to a temporary directory. A small local PowerShell updater waits for the running process to exit, replaces the portable application files and starts the new `ToastifyReloaded.exe`. User settings remain in `%APPDATA%` and are not overwritten.

The updater can be disabled from the application without disabling Spotify compatibility checks.

## Default hotkeys

| Action | Shortcut |
|---|---|
| Play / Pause | `Ctrl+Alt+Space` |
| Next track | `Ctrl+Alt+Right` |
| Previous track | `Ctrl+Alt+Left` |
| Volume up | `Ctrl+Alt+Up` |
| Volume down | `Ctrl+Alt+Down` |
| Mute | `Ctrl+Alt+M` |
| Seek +10s | `Ctrl+Alt+Shift+Right` |
| Seek -10s | `Ctrl+Alt+Shift+Left` |
| Show popup | `Ctrl+Alt+T` |

## Lyrics Plus

The **Lyrics** tab enables the `lyrics-plus` Custom App through Spicetify. This repository does not contain song lyrics.

Manual installation:

```powershell
.\scripts\install-lyrics.ps1 -InstallSpicetifyIfMissing
```

Manual post-update recovery remains available:

```powershell
.\scripts\restore-after-spotify-update.ps1
```

## Build from source

Requirements:

- Windows 10 or Windows 11
- .NET 8 SDK
- Git (recommended)

```powershell
git clone https://github.com/Marlius92/Toastify-Reloaded.git
cd Toastify-Reloaded
.\scripts\build.ps1
```

Self-contained x64 package:

```powershell
.\scripts\publish.ps1 -Runtime win-x64 -SelfContained $true
```

## GitHub Releases

Push a version tag, for example:

```powershell
git tag v1.1.0
git push origin v1.1.0
```

The release workflow publishes both x64 and ARM64 ZIPs. Those exact asset names are also used by the built-in updater.

## Repository structure

```text
Toastify-Reloaded/
├─ .github/workflows/
├─ docs/
├─ scripts/
├─ src/ToastifyReloaded/
│  ├─ Models/
│  ├─ Native/
│  └─ Services/
├─ CHANGELOG.md
├─ LICENSE
├─ README.md
└─ ToastifyReloaded.sln
```

## Notes and limitations

- The automatic updater assumes a portable installation location writable by the current user. If Toastify Reloaded is placed in a protected system directory, Windows permissions may prevent in-place updates.
- GitHub network access is required for Toastify Reloaded auto-update checks.
- Spicetify may temporarily lag behind a newly released Spotify version. In that case the Compatibility Guard intentionally stops retrying automatically for that version.
- Microsoft Store Spotify detection is supported for version reporting, but Spicetify itself recommends the regular desktop Spotify installation on Windows when Store-specific configuration problems occur.

## License

Toastify Reloaded source code is released under the MIT License. See `NOTICE.md` for third-party names and integration details.
