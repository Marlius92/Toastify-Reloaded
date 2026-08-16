# Changelog

## 1.0.3 - Full Rename

- Completed the full application identity rename to `ToastifyReloaded`.
- Renamed solution, project, source folder, namespaces, XAML class references and assembly identity.
- Moved settings to `%APPDATA%\ToastifyReloaded\settings.json`.
- Renamed the Windows startup registry value and single-instance mutex to `ToastifyReloaded`.
- Updated build/release workflows, scripts and documentation to use only Toastify Reloaded naming.

## 1.0.2 - Toastify Reloaded naming update

- Renamed the published executable to `ToastifyReloaded.exe`.
- Renamed GitHub build/release ZIPs to `ToastifyReloaded-win-x64.zip` and `ToastifyReloaded-win-arm64.zip`.
- GitHub Releases are now titled `Toastify Reloaded <tag>`.
- Updated visible application strings and project metadata from Toastify Reloaded to Toastify Reloaded.
- Kept the internal `ToastifyReloaded` namespace/source paths for compatibility and to minimize regression risk.

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
