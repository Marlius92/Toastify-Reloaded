# Changelog

## 1.3.4

### Wider, shorter Settings layout
- Refined the v1.3.3 Settings shell from 1000 × 760 to **1120 × 700**.
- Reduced unnecessary vertical footprint while adding horizontal breathing room for translated labels and wider controls.
- Expanded the adaptive Toast width grid and gives its right-hand panel extra horizontal space so Italian labels such as **Larghezza min** / **Larghezza max** no longer sit against or clip into the numeric controls.
- Preserved the fixed-size, no-user-scrollbar design and the existing Light/Dark tab structure.
- Kept all v1.3.3 localization and selected-tab readability fixes.


## 1.3.3

### Localization, dark tabs and layout
- Expanded the fixed Settings window to 1000 × 760 to prevent right-side clipping in Toast General and Colors & Font at common Windows DPI scales.
- Replaced the native TabItem header rendering with a theme-aware template so selected tabs remain readable in both Light and Dark themes, including the left-side Toast sub-tabs.
- Extended Italian localization from tab names to the complete static Settings interface: labels, group headers, checkboxes, buttons, combo options, help text, tooltips and Reloaded controls.
- Added runtime localization for generated monitor labels and tray menu commands.
- Added localized hotkey action labels through a binding converter.
- Preserved English as a fully reversible runtime language option.

## 1.3.2 - Preset workflow, optional song time and independent slide in/out

### Changed
- Removed the non-functional **Apply preset** button from `Toast -> Themes`.
- Selecting a built-in Toast theme now loads that preset directly into the editable controls; the change is persisted only when the main **Save** button is pressed.
- Saving settings no longer shows a success confirmation dialog. Validation/hotkey errors can still display warnings.

### Added
- Added optional **Show song time / duration** to the Toast (`current / total`, for example `1:26 / 3:20`).
- Enabled and completed the song progress bar so it reflects the current track timeline while the Toast is visible.
- Added independent **Slide In** and **Slide Out** directions.
- Added independent **Slide In** and **Slide Out** distances.
- Existing v1.3.x animation settings remain backward-compatible through the legacy direction/distance fields.


## 1.3.1 - Dark theme rendering fix

### Fixed
- Fixed the Dark application theme leaving the main TabControl/content surface white while text switched to a light foreground.
- Applied theme palette resources at both Window and Application scope so native WPF controls resolve dark SystemColors consistently.
- Added explicit dark-aware surfaces for TabControl, GroupBox, ComboBox, TextBox and ListBox.
- Added a dark-aware ComboBox template and dropdown item styling so closed controls and popup lists remain readable in Dark mode.
- Added dynamic hover, pressed and disabled foreground brushes for both Light and Dark themes.
- Preserved Light as the default historical appearance and kept toast themes independent from the application theme.

## 1.3.0

### Added
- Added Light, Dark and Follow Windows application themes.
- Expanded the fixed Settings window to 840x700 to accommodate Reloaded features without scrollbars.
- Added 13 built-in colored toast presets plus Custom.
- Added Fade, Slide, Fade + Slide and None animation modes.
- Added position presets and multi-monitor targeting.
- Added JSON settings import/export.
- Added advanced diagnostics and compatibility report export.
- Added English/Italian localization infrastructure.

## 1.2.3

- Added optional automatic toast width based on the real track title and artist text.
- Added configurable minimum and maximum automatic toast widths (250 / 600 px defaults).
- Added album-cover artwork from the Windows Spotify media session.
- Added Toastify Reloaded icon fallback when Spotify artwork is unavailable.
- Added selectable toast image mode: Album cover, Toastify Reloaded icon, or None.
- Preserved the historical 60 x 60 artwork slot and 250 x 70 minimum classic toast geometry.
- Kept manual Toast Width behavior available when automatic width is disabled.

## 1.2.2 - 2026-08-16

### Fixed
- Reworked the Toast settings layouts to prevent labels, numeric controls and color/font controls from overlapping at Windows DPI scaling while keeping the historical 580x570 shell and native control style.
- Made the added Reloaded tab vertically scrollable so Compatibility Guard, update controls and Tools never clip each other.
- Reworked Toast corner-radius and color/font grids into explicit rows/columns instead of overlapping absolute margins.

### Added
- Added independent **Fade In** and **Fade Out** times in `Toast -> General`, each configurable from 0 to 5000 ms.
- Added real opacity animation for toast fade-in and fade-out while preserving the classic 250x70 default toast geometry.
- Decoupled global hotkeys from the visible Settings window by using a dedicated hidden Win32/WPF message sink. Hotkeys remain active while any other application is in the foreground and while the Settings window is hidden in the tray.

## 1.2.1 - 2026-08-16

### Fixed
- Replaced the earlier visual approximation with a strict Toastify 1.11.2-reference WPF interface: 580x570, non-resizable, native Windows controls and the historical General / Hotkeys / Toast / Advanced tab order.
- Restored the historical 250x70 toast notification geometry, gradient, 60x60 artwork area, title typography, border and progress-bar layout.
- Removed the custom dark dashboard visual language introduced by earlier Reloaded builds.
- Restored native WPF control styling and the historical settings geometry rather than a Reloaded reinterpretation.

### Added
- Added one extra **Reloaded** tab for Lyrics Plus, automatic updates and Compatibility Guard without modifying the original four-tab layout.
- Added persisted classic Toast size, position, colors, font and border settings.
- Added the historical Toastify-style logo resource and application icon.

### Packaging
- Keeps the v1.2 installer model: installed application under Program Files, Start-menu entry, uninstall support and installer-based automatic updates.

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
