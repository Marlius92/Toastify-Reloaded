# Changelog

## 1.2.0 - 2026-08-16

### Added
- Added a real Windows installer based on NSIS, matching the installer technology used by the historical Toastify setup.
- Added x64 and ARM64 Setup packages: `ToastifyReloaded-Setup-win-x64.exe` and `ToastifyReloaded-Setup-win-arm64.exe`.
- Added installation under `C:\Program Files\Toastify Reloaded`.
- Added Start-menu shortcuts, Windows Installed Apps registration and `Uninstall.exe`.
- Added clean upgrade-in-place behavior while preserving `%APPDATA%\ToastifyReloaded` settings.
- Added automatic installer-based updates: the app downloads the correct Setup, requests UAC, closes the running version, upgrades and restarts.
- Added `scripts/build-installer.ps1` and `installer/ToastifyReloaded.nsi`.
- GitHub CI now builds an actual installer on every main-branch build.

### Changed
- Public GitHub Releases now publish Setup executables instead of portable ZIP packages.
- Maintenance PowerShell helpers remain embedded in `ToastifyReloaded.exe`; no `scripts` directory is installed or shipped to end users.
- The main GUI remains the original Toastify Reloaded interface, with only the dedicated **Aggiornamenti** tab added in 1.1.3.
- Internal application version bumped to `1.2.0`.

### Migration note
- Portable 1.1.x builds require one manual installation of the 1.2.0 Setup. After that, future updates use the installer automatically.
- The installer intentionally does not delete an old portable EXE outside Program Files.

## 1.1.3 - 2026-08-16

### Fixed
- Restored the original Toastify Reloaded main interface.
- Restored the original window dimensions, header, layout, colors and tray menu.
- Removed Compatibility Guard visual changes outside the dedicated tab.

### Added
- Added only the **Aggiornamenti** tab, matching the original interface style, for auto-update, Compatibility Guard and Spotify/Spicetify/Lyrics repair.
- Automatic functions continue to operate in the background without requiring that tab to be opened.

## 1.1.2 - Release/version alignment
- Aligned executable, GitHub tag and Release versioning.
- Kept Single-EXE packaging and embedded maintenance helpers.

## 1.1.1 - Single-EXE release packaging
- Changed public x64 and ARM64 Release ZIPs to contain only `ToastifyReloaded.exe`.
- Embedded Lyrics, recovery, removal and diagnostic PowerShell helpers inside the application assembly.

## 1.1.0 - Automatic Compatibility Guard
- Added automatic Spotify version detection and repair after Spotify updates.
- Added Lyrics Plus preservation, Spicetify recovery and GitHub self-update support.

## 1.0.3 - Full Rename
- Completed the full application identity rename to `ToastifyReloaded`.

## 1.0.2 - Toastify Reloaded naming update
- Renamed executable and release artifacts to Toastify Reloaded.

## 1.0.1 - 2026-08-16
- Fixed Windows CI compilation errors and updated GitHub Actions.

## 1.0.0 - 2026-08-16
- First public repository structure and core Spotify controls.
