# Toastify Reloaded

<p align="center">
  <img src="assets/readme/toastify-reloaded-overview.png" alt="Toastify Reloaded overview" width="100%">
</p>

<p align="center">
  <a href="https://github.com/Marlius92/Toastify-Reloaded/actions/workflows/build.yml"><img alt="Build" src="https://github.com/Marlius92/Toastify-Reloaded/actions/workflows/build.yml/badge.svg"></a>
  <a href="https://github.com/Marlius92/Toastify-Reloaded/releases/latest"><img alt="Latest release" src="https://img.shields.io/github/v/release/Marlius92/Toastify-Reloaded?display_name=tag&sort=semver"></a>
  <a href="LICENSE"><img alt="License" src="https://img.shields.io/github/license/Marlius92/Toastify-Reloaded"></a>
  <img alt="Windows 10/11" src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4?logo=windows&logoColor=white">
  <img alt=".NET 8" src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white">
</p>

<p align="center">
  <strong>Classic Toastify experience. Modern Spotify compatibility.</strong>
</p>

**Toastify Reloaded** is a modern continuation of the classic **Toastify for Spotify** experience on Windows. It preserves the familiar Toastify interface and workflow while replacing obsolete integration points with modern Windows media controls, persistent global hotkeys, Spicetify/Lyrics support, automatic recovery after Spotify updates and installable Windows releases.

> **Independent community project.** Toastify Reloaded is not an official release from Spotify AB or from the original Toastify developers.

<p align="center">
  <a href="https://github.com/Marlius92/Toastify-Reloaded/releases/latest"><strong>⬇ Download the latest Toastify Reloaded release</strong></a>
</p>

For most Windows PCs, download **`ToastifyReloaded-Setup-win-x64.exe`**. ARM64 devices should use **`ToastifyReloaded-Setup-win-arm64.exe`**.

---

## Feature tour

<p align="center">
  <img src="assets/readme/toastify-reloaded-feature-tour.png" alt="Toastify Reloaded feature tour" width="100%">
</p>

> The images above are documentation/promotional composites. Real screenshots from stable builds can be added alongside them as the UI is finalized.

## Main features

- **Persistent global hotkeys** that keep working while Spotify, a game, a browser or another application is in the foreground.
- Play/Pause, Next, Previous, seek forward/backward, volume and mute controls.
- Classic Toastify-style notification popup with configurable size, position, border, colors, fonts and progress bar.
- Independently configurable **Fade In** and **Fade Out** timings.
- System-tray operation and optional startup with Windows.
- No Spotify password, Spotify account credentials or Spotify developer API key required for the media-control core.
- One-click **Lyrics Plus** integration through Spicetify.
- **Compatibility Guard** that detects Spotify version changes and can repair Spicetify automatically.
- Automatic fallback recovery if the normal post-update repair fails.
- Repair-loop protection when a new Spotify version is not yet supported by Spicetify.
- Automatic Toastify Reloaded update checks through GitHub Releases.
- Native Windows Setup packages for **x64** and **ARM64**.
- Standard Windows installation, Start-menu entry and clean uninstall support.
- Local user configuration stored in `%APPDATA%\ToastifyReloaded`.

## Classic interface

Toastify Reloaded intentionally preserves the historical Toastify desktop experience instead of replacing it with a modern dashboard.

The classic top-level structure remains recognizable:

`General` · `Hotkeys` · `Toast` · `Advanced` · `Reloaded`

The **Reloaded** tab contains the features that did not exist in the historical application, such as Lyrics Plus maintenance, Compatibility Guard, diagnostics and update controls. The original-style tabs remain focused on the classic Toastify workflow.

The UI is also being made DPI-safe so that labels, numeric controls and configuration fields do not overlap at common Windows scaling levels.

## Original Toastify vs Toastify Reloaded

| Capability | Original Toastify | Toastify Reloaded |
|---|:---:|:---:|
| Classic Toastify settings workflow | ✓ | ✓ |
| Global media hotkeys | ✓ | ✓ |
| Toast notification customization | ✓ | ✓ |
| Windows installer | ✓ | ✓ x64 / ARM64 |
| Automatic application updates | ✓ | ✓ GitHub Setup updater |
| Proxy configuration | ✓ | Removed |
| Modern Windows media-session control | — | ✓ |
| Fade In / Fade Out controls | — | ✓ |
| Lyrics Plus / Spicetify integration | — | ✓ |
| Spotify-update Compatibility Guard | — | ✓ |
| Automatic Spicetify repair | — | ✓ |
| Repair-loop protection | — | ✓ |

## Global hotkeys

Global shortcuts are registered through a dedicated hidden native message window. They are not tied to the visible Settings window, so they continue to work when Toastify Reloaded is minimized to the tray or another application has focus.

Default mappings can include:

| Action | Default shortcut |
|---|---|
| Play / Pause | `Ctrl+Alt+Space` |
| Next track | `Ctrl+Alt+Right` |
| Previous track | `Ctrl+Alt+Left` |
| Volume up | `Ctrl+Alt+Up` |
| Volume down | `Ctrl+Alt+Down` |
| Mute | `Ctrl+Alt+M` |
| Seek +10s | `Ctrl+Alt+Shift+Right` |
| Seek -10s | `Ctrl+Alt+Shift+Left` |
| Show toast | `Ctrl+Alt+T` |

Windows-reserved shortcuts or combinations already registered by another application may still be unavailable.

## Toast customization

The **Toast** tab keeps the historical customization model while extending it with modern options.

You can configure:

- toast width and height;
- screen position;
- display time;
- border thickness and corner radius;
- background gradient and offsets;
- border color;
- title/subtitle fonts and colors;
- drop shadow, depth and blur;
- progress-bar colors;
- **Fade In** duration;
- **Fade Out** duration.

`0 ms` disables the corresponding fade animation. **Display Time** remains separate from the two animation durations.

## Lyrics Plus

Toastify Reloaded can enable and maintain the **Lyrics Plus** Custom App through Spicetify.

The project does **not** contain, scrape or redistribute song lyrics. Lyrics availability depends on the providers supported by Lyrics Plus/Spicetify.

After Spotify updates itself, Toastify Reloaded can automatically restore the injected Spicetify interface so that the Lyrics entry remains available.

## Compatibility Guard

Spotify updates frequently change files that Spicetify customizes. Toastify Reloaded therefore records the detected Spotify version and checks for changes.

When a new Spotify version is detected, Compatibility Guard can:

1. close Spotify;
2. optionally upgrade the Spicetify CLI;
3. run the standard `spicetify backup apply` recovery;
4. fall back to `spicetify restore backup apply` if required;
5. make sure Lyrics Plus remains enabled;
6. reapply Spicetify;
7. restart Spotify;
8. record the new version only after successful recovery.

If Spicetify does not yet support a new Spotify build, Toastify Reloaded avoids repeating the same failed repair forever. A manual retry remains available later.

## Installation

Toastify Reloaded is distributed as a normal Windows application rather than a loose portable folder.

GitHub Releases provide:

- **`ToastifyReloaded-Setup-win-x64.exe`** — standard Intel/AMD 64-bit Windows PCs.
- **`ToastifyReloaded-Setup-win-arm64.exe`** — Windows on ARM devices.

The installer places the application under `Program Files`, creates Start-menu integration, registers Toastify Reloaded in Windows **Installed apps** and provides a standard uninstaller.

User preferences remain under:

```text
%APPDATA%\ToastifyReloaded
```

so normal upgrades do not wipe your settings.

## Compatibility

| Platform / component | Status |
|---|---|
| Windows 11 x64 | Supported |
| Windows 10 x64 | Supported target |
| Windows ARM64 | Installer/build supported |
| Spotify Desktop for Windows | Supported target |
| Spotify Microsoft Store build | Media detection may work; Spicetify compatibility can differ |
| Spicetify | Required only for Lyrics/custom Spotify UI features |
| .NET runtime on user PC | Not required when using the self-contained installer build |

## Privacy

Toastify Reloaded is designed to keep its media-control core local.

- It does **not** need your Spotify password.
- It does **not** store Spotify login credentials.
- It does **not** require a Spotify developer API key for normal media controls.
- Playback control and metadata primarily use Windows media-session APIs.
- Settings are stored locally in `%APPDATA%\ToastifyReloaded`.
- Internet access is used for functions such as GitHub update checks and optional Spicetify-related downloads/updates.

## Known limitations

- A brand-new Spotify release may temporarily be unsupported by Spicetify.
- Windows can reject a global hotkey if another process already owns that key combination.
- Windows theme, font metrics and DPI scaling can produce small visual differences compared with historical Toastify screenshots.
- Lyrics availability depends on Lyrics Plus/Spicetify providers and is separate from Toastify Reloaded itself.
- Installer updates can trigger a normal Windows UAC prompt because the application is installed under `Program Files`.

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

## Reporting bugs

If Spotify updates or something stops working, open a GitHub Issue:

**https://github.com/Marlius92/Toastify-Reloaded/issues**

Useful information to include:

- Toastify Reloaded version;
- Windows version and architecture;
- Spotify version;
- Spicetify version, if applicable;
- what you expected to happen;
- what actually happened;
- screenshot or GitHub Actions/build error when relevant.

Do not include passwords, tokens or other private credentials.

## Roadmap

### Completed / implemented

- [x] Historical Toastify-style interface
- [x] Modern Windows media-session backend
- [x] Persistent global hotkeys
- [x] Customizable toast notifications
- [x] Fade In / Fade Out configuration
- [x] Lyrics Plus integration
- [x] Compatibility Guard
- [x] Automatic Spotify/Spicetify repair workflow
- [x] Windows x64 and ARM64 installers
- [x] GitHub-based application updater
- [x] Proxy settings removed from Reloaded UI
- [x] DPI/overlap hardening of the historical settings pages

### Possible future improvements

- [ ] Additional toast animation styles
- [ ] More positioning presets and multi-monitor options
- [ ] Import/export of Toastify Reloaded settings
- [ ] Localization / additional interface languages
- [ ] Additional diagnostics and compatibility reporting
- [ ] More automated visual-regression checks for the historical UI

## Build from source

Requirements:

- Windows 10 or Windows 11;
- .NET 8 SDK;
- Git;
- NSIS when building the installer.

```powershell
git clone https://github.com/Marlius92/Toastify-Reloaded.git
cd Toastify-Reloaded
.\scripts\build.ps1
```

Build the x64 installer:

```powershell
.\scripts\build-installer.ps1 -Runtime win-x64
```

## Repository structure

```text
Toastify-Reloaded/
├─ .github/workflows/        CI and release automation
├─ assets/readme/            README images and documentation visuals
├─ docs/                     Documentation
├─ installer/                NSIS Windows installer
├─ scripts/                  Build and developer/maintenance tools
├─ src/ToastifyReloaded/     Application source code
├─ CHANGELOG.md
├─ LICENSE
├─ NOTICE.md
├─ README.md
└─ ToastifyReloaded.sln
```

## License

Toastify Reloaded is distributed under the **GNU General Public License v2.0 (GPL-2.0)** to remain compatible with the historical Toastify code/resource lineage used by the classic interface.

See [`LICENSE`](LICENSE) and [`NOTICE.md`](NOTICE.md) for details and attribution information.

Spotify, the Spotify logo and related trademarks are property of Spotify AB. Toastify Reloaded is an independent project and is not affiliated with, sponsored by or endorsed by Spotify AB or by the original Toastify developers.
