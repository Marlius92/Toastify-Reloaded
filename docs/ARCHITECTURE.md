# Architecture

## Goals

- Avoid dependency on Spotify window titles or UI automation.
- Avoid storing Spotify credentials.
- Keep the Spotify UI customization (Lyrics) isolated from playback control.
- Keep the desktop app dependency-light: .NET 8 + Windows APIs only.

## Components

### `SpotifySessionService`

Uses `Windows.Media.Control.GlobalSystemMediaTransportControlsSessionManager` to discover the Spotify media session, read metadata, control playback and seek.

### `GlobalHotkeyService`

Uses Win32 `RegisterHotKey`/`UnregisterHotKey` and a WPF `HwndSource` hook to receive `WM_HOTKEY` messages.

### `MediaKeyService`

Uses Win32 `SendInput` for volume up/down/mute keys.

### `SettingsService`

Serializes settings to `%APPDATA%\ToastifyReloaded\settings.json` with `System.Text.Json`.

### `StartupService`

Manages the current user's `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` entry.

### `ToastWindow`

Small borderless WPF window positioned in the bottom-right of the working area. It is intentionally separate from Windows Action Center notifications so the visual behavior remains deterministic.

### Lyrics scripts

Lyrics is deliberately not implemented in the WPF process. PowerShell calls the local Spicetify CLI and enables its built-in `lyrics-plus` Custom App. This means the project does not need to redistribute or fork that app.

## Data flow

```text
Keyboard
  -> RegisterHotKey / WM_HOTKEY
  -> GlobalHotkeyService
  -> MainWindow action dispatcher
  -> SpotifySessionService OR MediaKeyService
  -> Windows media session / Windows audio keys

Spotify media session
  -> polling
  -> TrackInfo
  -> change detection
  -> ToastWindow

Lyrics tab
  -> PowerShellService
  -> scripts/*.ps1
  -> spicetify CLI
  -> Spotify UI customization
```


## v1.1.0 - Maintenance layer

`SpotifyInstallationService` identifica la versione Spotify; `CompatibilityRepairService` gestisce il recupero Spicetify; `UpdateService` consulta GitHub Releases e prepara l'aggiornamento in-place della distribuzione portable. Le impostazioni persistenti e lo stato anti-loop sono in `AppSettings`.
