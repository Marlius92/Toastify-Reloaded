# Toastify Reloaded

**Toastify Reloaded** is a modern continuation of the classic **Toastify for Spotify** experience on Windows. It preserves the familiar Toastify interface and workflow while replacing obsolete integration points with modern Windows media controls, global hotkeys, Spicetify/Lyrics support, automatic repair after Spotify updates and installable Windows releases.

> Independent community project. Toastify Reloaded is not an official release from Spotify AB or from the original Toastify developers.

## Project lineage & credits

Toastify Reloaded exists because of the work of the developers who created and maintained the original Toastify project. This repository is intended to preserve that experience while adapting it to current versions of Spotify and Windows.

Special credit goes to:

- **nachmore** — creator of the original Toastify project: [nachmore/toastify](https://github.com/nachmore/toastify)
- **Alessandro Attard Barbini (`@aleab`)** — principal maintainer of the later Toastify fork and the historical **1.11.x** generation used as the main visual and behavioral reference for Toastify Reloaded: [aleab/toastify](https://github.com/aleab/toastify)
- **Marlius92** — development and maintenance of **Toastify Reloaded**.

The original authors are **not responsible for Toastify Reloaded**, its new code, support, releases or future changes.

### Project philosophy

Toastify Reloaded is not intended to redesign Toastify. Its goal is to keep the classic application recognizable and familiar while modernizing the parts that no longer work reliably with current Spotify and Windows versions.

**Classic Toastify experience. Modern Spotify compatibility.**

## Main features

- Global hotkeys even while Spotify or any other application is in the foreground; registration is independent from the visible Settings window.
- Play/Pause, Next and Previous track.
- Seek forward/backward by 10 seconds.
- Windows volume up/down and mute.
- Customizable shortcuts.
- Popup when the current track changes, with independently configurable fade-in and fade-out timing.
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
- Automatic Toastify Reloaded updates through signed-style Windows Setup packages from this repository's GitHub Releases.
- Native installer packages for Windows x64 and ARM64, with Start menu and uninstall registration.
- Local JSON configuration in `%APPDATA%\ToastifyReloaded\settings.json`.

## Classic Toastify 1.11.2 interface (v1.2.2)

The visible shell is intentionally based on the historical Toastify 1.11.2 desktop interface rather than the custom Reloaded dashboard used by earlier builds.

The compatibility target is deliberately strict:

- Settings window: **580 × 570**, fixed size, native WPF/Windows controls.
- Historical top-level tabs remain in the same order: **General**, **Hotkeys**, **Toast**, **Advanced**.
- Historical control geometry, labels, logo placement, Save button and Default split-button are preserved; internal grids/scrolling were made DPI-safe in v1.2.2 to prevent text/control overlap.
- The Toast popup returns to **250 × 70**, with the original gray-to-black gradient, 60 × 60 artwork area, border, typography and progress-bar geometry.
- Reloaded-specific functions are isolated in one additional **Reloaded** tab so they do not redesign the historical four tabs.
- The installer, Compatibility Guard, Spicetify/Lyrics repair and GitHub updater remain modern backend components and do not change the classic visual shell.

Because WPF uses the active Windows system theme and DPI scaling, exact physical pixels can vary with Windows theme/DPI. The XAML geometry and control structure are kept aligned with the historical 1.11.2 reference.

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

If Spicetify does not yet support a brand-new Spotify release, Toastify Reloaded reports the failure and avoids an automatic retry loop for that same version. The user can force another attempt later from **Reloaded → Repair now**.

## Installation and automatic updates

Starting with **v1.2.0**, Toastify Reloaded is distributed as a normal Windows application instead of a portable ZIP. GitHub Releases provide:

- `ToastifyReloaded-Setup-win-x64.exe`
- `ToastifyReloaded-Setup-win-arm64.exe`

The Setup installs Toastify Reloaded in `C:\Program Files\Toastify Reloaded`, creates Start-menu shortcuts, registers the app in Windows **Installed apps**, and provides a normal uninstaller. Maintenance scripts remain embedded inside `ToastifyReloaded.exe`; there is no public `scripts` folder.

The built-in updater now downloads the matching Setup executable. Windows displays the normal UAC confirmation, the installer waits for the running Toastify process to close, updates the installed files and restarts the application. User settings remain in `%APPDATA%\ToastifyReloaded` and are preserved.

> **One-time migration:** users of portable 1.1.x builds should manually run the v1.2.0 Setup once. Future versions then update through the installed-app mechanism.

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

Global hotkeys are registered through a dedicated hidden native message window, so they continue to work when another program is active or the Settings window is hidden in the system tray. Windows-reserved or already-registered key combinations can still be unavailable.

### Toast fade

`Toast -> General` includes separate **Fade In** and **Fade Out** values in milliseconds. A value of `0` disables that side of the animation. `Display Time` remains the amount of time the toast stays fully visible between the two animations.

## Lyrics Plus

The **Reloaded** tab contains the **Lyrics Plus** section that enables the `lyrics-plus` Custom App through Spicetify. This repository does not contain song lyrics.

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

Build the installable x64 Setup:

```powershell
.\scripts\build-installer.ps1 -Runtime win-x64
```

NSIS must be installed on the build machine.

## GitHub Releases

Push a version tag, for example:

```powershell
git tag v1.2.2
git push origin v1.2.2
```

The release workflow publishes x64 and ARM64 Setup executables. Those installer asset names are also used by the built-in updater.

## Repository structure

```text
Toastify-Reloaded/
├─ .github/workflows/
├─ docs/
├─ installer/
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

- Installed updates use the Windows UAC prompt because the default install path is under Program Files.
- GitHub network access is required for Toastify Reloaded auto-update checks.
- Spicetify may temporarily lag behind a newly released Spotify version. In that case the Compatibility Guard intentionally stops retrying automatically for that version.
- Microsoft Store Spotify detection is supported for version reporting, but Spicetify itself recommends the regular desktop Spotify installation on Windows when Store-specific configuration problems occur.

## License & trademark notice

Toastify Reloaded v1.2.1 and later is distributed under the **GNU General Public License v2.0 (GPL-2.0)** to remain compatible with the historical Toastify 1.11.2 code/UI/resource lineage used by the classic interface. See `LICENSE` and `NOTICE.md`.

The Toastify name and historical project credits belong to their respective authors. Spotify, the Spotify logo and related trademarks are property of Spotify AB. Toastify Reloaded is an independent open-source project and is not affiliated with, sponsored by or endorsed by Spotify AB or the original Toastify developers.
