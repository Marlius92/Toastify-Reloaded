# Toastify Reloaded

**Toastify Reloaded** is a lightweight Windows companion for Spotify focused on global keyboard shortcuts, track-change popups and a reproducible Lyrics setup through Spicetify.

> Independent community project. Not affiliated with Spotify AB. Toastify Reloaded is a modern reimplementation inspired by the discontinued Toastify utility.

## Main features

- Global hotkeys even while Spotify is in the background.
- Play/Pause, Next and Previous track.
- Seek forward/backward by 10 seconds.
- Windows volume up/down and mute.
- Customizable shortcut strings such as `Ctrl+Alt+Space`.
- Popup when the current track changes.
- Manual “show current track” popup.
- System-tray operation.
- Optional launch with Windows.
- Local JSON configuration in `%APPDATA%\ToastifyModern\settings.json`.
- No Spotify username/password or developer API key required.
- One-click scripts for **Lyrics Plus** through Spicetify.
- Recovery helper after Spotify updates.
- GitHub Actions builds for Windows.

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

Hotkeys can be edited from the application.

## How it works

Toastify Modern does not automate the Spotify window and does not need to locate a button inside Spotify. Playback data and commands are obtained from the Windows `GlobalSystemMediaTransportControlsSessionManager`. Global shortcuts are registered with the Win32 `RegisterHotKey` API.

This approach is intentionally separate from the Lyrics modification. An update to Spotify can therefore remove Spicetify's injected UI while Toastify Modern itself continues to run.

## Lyrics Plus

The **Lyrics** tab enables the `lyrics-plus` Custom App that ships with Spicetify. The repository does not contain song lyrics and does not copy the Lyrics Plus source code.

From PowerShell you can also run:

```powershell
.\scripts\install-lyrics.ps1 -InstallSpicetifyIfMissing
```

After a Spotify update, if the Lyrics entry disappears:

```powershell
.\scripts\restore-after-spotify-update.ps1
```

The underlying standard Spicetify recovery command is:

```powershell
spicetify backup apply
```

## Build from source

Requirements for developers:

- Windows 10 or Windows 11
- .NET 8 SDK
- Git (recommended)

```powershell
git clone https://github.com/Marlius92/Toastify-Reloaded.git
cd Toastify-Reloaded
.\scripts\build.ps1
```

To create a self-contained ZIP that does not require the .NET runtime on the destination PC:

```powershell
.\scripts\publish.ps1 -Runtime win-x64 -SelfContained $true
```

The output is created in `dist\`.

## GitHub Releases

Push a tag such as:

```powershell
git tag v1.0.0
git push origin v1.0.0
```

The `release.yml` workflow builds both x64 and ARM64 ZIP files and attaches them to a GitHub Release.

## Repository structure

```text
ToastifyModern/
├─ .github/workflows/      CI and release automation
├─ docs/                   Detailed Italian documentation
├─ scripts/                Build, publish, Lyrics and diagnostic helpers
├─ src/ToastifyModern/     WPF/.NET 8 source code
├─ CHANGELOG.md
├─ CONTRIBUTING.md
├─ LICENSE
├─ NOTICE.md
├─ README.md
├─ SECURITY.md
└─ ToastifyModern.sln
```

## Troubleshooting

### Spotify is not detected

1. Open Spotify and start a track.
2. Press **Aggiorna** in Toastify Modern.
3. Run `scripts\diagnose.ps1` if it is still not detected.

### A hotkey does not work

Another application or Windows may already own the same shortcut. Change the shortcut and save again.

### Lyrics disappeared after Windows/Spotify updates

This is normally the injected Spicetify UI being replaced by a Spotify update. Run:

```powershell
.\scripts\restore-after-spotify-update.ps1
```

### Lyrics Plus is enabled but no lyrics load

That is separate from Toastify Modern itself. Open the Lyrics Plus settings inside Spotify and check the configured provider.

## License

Toastify Reloaded source code is released under the MIT License. See `NOTICE.md` for third-party names and integration details.
