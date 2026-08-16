# Changelog

## 1.1.0 - Automatic Compatibility Guard

- Added automatic Spotify version detection and baseline tracking.
- Added a five-minute compatibility check while Toastify Reloaded is running.
- Added automatic recovery when a new Spotify version is detected.
- Recovery uses the official post-update `spicetify backup apply` command.
- Added `spicetify restore backup apply` fallback for failed standard recovery.
- Added optional `spicetify upgrade` before recovery.
- Added automatic Lyrics Plus preservation during recovery.
- Added automatic Spotify restart through `spicetify auto`.
- Added loop protection: a failed automatic repair is attempted only once per Spotify version unless manually forced.
- Added a new **Aggiornamenti** tab with compatibility state and maintenance controls.
- Added automatic Toastify Reloaded update checks using GitHub Releases.
- Added architecture-aware x64/ARM64 asset selection.
- Added a self-updater that replaces portable app files after the running process exits and restarts Toastify Reloaded.
- Updated the interactive recovery script to use `spicetify upgrade` instead of the theme hot-reload `spicetify update` command.
- Bumped application version to 1.1.0.

## 1.0.3 - Full Rename

- Completed the full application identity rename to `ToastifyReloaded`.
- Renamed solution, project, source folder, namespaces, XAML class references and assembly identity.
- Moved settings to `%APPDATA%\ToastifyReloaded\settings.json`.
- Renamed the Windows startup registry value and single-instance mutex to `ToastifyReloaded`.
- Updated build/release workflows, scripts and documentation to use only Toastify Reloaded naming.

## 1.0.2 - Toastify Reloaded naming update

- Renamed the published executable to `ToastifyReloaded.exe`.
- Renamed GitHub build/release ZIPs to `ToastifyReloaded-win-x64.zip` and `ToastifyReloaded-win-arm64.zip`.
- GitHub Releases are titled `Toastify Reloaded <tag>`.
- Updated visible application strings and project metadata.

## 1.0.1 - 2026-08-16

- Fixed Windows CI compilation errors by adding explicit `System.IO` imports.
- Updated GitHub Actions checkout/setup-dotnet actions to Node 24-compatible major versions.
- Updated README branding and the clone URL for `Marlius92/Toastify-Reloaded`.

## 1.0.0 - 2026-08-16

- First public repository structure.
- Global configurable hotkeys.
- Spotify play/pause, next, previous and ±10-second seeking through Windows media sessions.
- Windows volume up/down/mute hotkeys.
- Track-change popup.
- System tray support.
- Start-with-Windows option.
- Spicetify Lyrics Plus install/restore/remove helpers.
- GitHub Actions build and release workflows.
