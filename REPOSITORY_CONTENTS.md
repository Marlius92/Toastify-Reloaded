# Repository contents

This package is ready to be committed as a Git repository.

## Source

- `src/ToastifyModern/App.xaml(.cs)` — application startup and single-instance guard
- `src/ToastifyModern/MainWindow.xaml(.cs)` — settings UI, tray, dispatch and polling
- `src/ToastifyModern/ToastWindow.xaml(.cs)` — track popup
- `src/ToastifyModern/Models/*` — settings, hotkeys and track model
- `src/ToastifyModern/Services/*` — Spotify media session, hotkeys, media keys, settings, startup and PowerShell integration
- `src/ToastifyModern/Native/NativeMethods.cs` — Win32 interop declarations

## PowerShell

- `scripts/build.ps1` — build
- `scripts/publish.ps1` — self-contained packages
- `scripts/install-lyrics.ps1` — install/enable Lyrics Plus
- `scripts/restore-after-spotify-update.ps1` — reapply Spicetify after Spotify updates
- `scripts/remove-lyrics.ps1` — disable Lyrics Plus
- `scripts/diagnose.ps1` — environment report

## Documentation

- `README.md` — GitHub landing page
- `docs/GUIDA_COMPLETA_IT.md` — complete Italian manual
- `docs/ARCHITECTURE.md` — technical architecture
- `docs/LYRICS.md` — Lyrics details
- `docs/GITHUB_PUBLISHING.md` — publishing and releases
- `CHANGELOG.md`, `CONTRIBUTING.md`, `SECURITY.md`, `NOTICE.md`

## Automation

- `.github/workflows/build.yml`
- `.github/workflows/release.yml`
