# Toastify Reloaded

<p align="center">
  <strong>Classic Toastify experience. Modern Spotify compatibility.</strong>
</p>

<p align="center">
  <a href="https://github.com/Marlius92/Toastify-Reloaded/actions/workflows/build.yml">
    <img alt="Build" src="https://github.com/Marlius92/Toastify-Reloaded/actions/workflows/build.yml/badge.svg">
  </a>
  <a href="https://github.com/Marlius92/Toastify-Reloaded/releases/latest">
    <img alt="Latest release" src="https://img.shields.io/github/v/release/Marlius92/Toastify-Reloaded?display_name=tag">
  </a>
  <a href="LICENSE">
    <img alt="License GPL-2.0" src="https://img.shields.io/badge/license-GPL--2.0-blue">
  </a>
  <img alt=".NET 8" src="https://img.shields.io/badge/.NET-8.0-512BD4">
  <img alt="Windows" src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4">
</p>

**Toastify Reloaded** is a modern continuation of the classic Toastify experience for Spotify on Windows.

The project keeps the recognizable Toastify workflow and notification style while replacing obsolete integration points with modern Windows media controls, global hotkeys, Spicetify/Lyrics support, automatic repair after Spotify updates, installable Windows releases, dark mode, toast themes, adaptive song timeline controls, animations and multi-monitor positioning.

> **Independent community project.** Toastify Reloaded is not affiliated with Spotify AB and is not an official release maintained by the original Toastify developers.

**Current documented feature set: v1.3.2.**

---

## Download

### Latest release

**[Download Toastify Reloaded from GitHub Releases](https://github.com/Marlius92/Toastify-Reloaded/releases/latest)**

Windows installer packages are provided for:

- **Windows x64** — `ToastifyReloaded-Setup-win-x64.exe`
- **Windows ARM64** — `ToastifyReloaded-Setup-win-arm64.exe`

Toastify Reloaded installs as a normal Windows application, with Start-menu integration, Installed Apps registration, uninstall support and in-place updates.

---

# What's new in v1.3.2

Version **1.3.2** refines the v1.3 customization system and simplifies how toast settings are applied.

### Theme preset workflow

- The separate **Apply preset** button has been removed.
- Selecting a built-in toast preset immediately loads its values into the existing color controls.
- You can still modify every loaded value manually.
- **Save** is now the single action used to persist settings.
- Successful saves are silent; the old confirmation popup is no longer shown.
- Error messages are still shown when something genuinely needs attention.

### Optional song timeline

The toast timeline is now fully optional and split into independent controls:

- **Show song progress bar**
- **Show song time / duration**

The time display uses the familiar format:

```text
1:26 / 3:20
```

The progress bar now follows the real playback timeline while the toast is visible.

You can enable:

- only the progress bar;
- only current time / total duration;
- both;
- neither.

### Independent Slide In / Slide Out

Slide animation entry and exit behavior can now be configured separately.

Available directions:

- Up
- Down
- Left
- Right

You can independently configure:

- **Slide In direction**
- **Slide Out direction**
- **Slide In distance**
- **Slide Out distance**
- Enter / Fade In duration
- Exit / Fade Out duration

Example:

```text
Animation: Fade + Slide
Slide In:  Up      / 28 px
Slide Out: Right   / 60 px
Enter: 250 ms
Exit:  250 ms
```

This allows the toast to enter from one side and leave in a completely different direction.

---

# v1.3.1 — Dark theme correction

Version **1.3.1** corrected the first v1.3.0 Dark application theme implementation.

The dark palette is now applied consistently to:

- main window surfaces;
- tabs and tab contents;
- group boxes;
- text boxes;
- combo boxes and their dropdowns;
- list controls;
- borders;
- disabled states;
- selection states;
- supported title-bar surfaces.

The historical **Light** theme remains available and independent from the Dark theme.

---

# What's new in v1.3.0

Version **1.3.0** is the largest Reloaded UI and customization update so far.

### Application interface

- Larger fixed Settings window to prevent crowded controls and unnecessary scrollbars.
- **Light** application theme.
- **Dark** application theme.
- **Follow Windows** theme mode.
- Dark title-bar integration where supported by Windows.
- DPI-safe layouts.
- English and Italian localization infrastructure.

### Toast customization

- Built-in colored theme presets.
- Manual color customization remains available.
- Album artwork support.
- Toastify Reloaded icon fallback when album artwork is unavailable.
- Optional image-free toast.
- Automatic toast width based on title and artist length.
- Configurable minimum and maximum toast width.
- Fade In and Fade Out timing.
- Slide animations.
- Fade + Slide animations.
- Animation direction and distance settings.
- Position presets.
- Multi-monitor selection.
- Custom screen coordinates.

### Settings & diagnostics

- Import settings from JSON.
- Export settings to JSON.
- Advanced compatibility diagnostics.
- Copy/export diagnostic reports.
- Compatibility Guard status reporting.

---

# Main features

## Global hotkeys

Hotkeys are registered independently from the visible Settings window and continue to work while another program is focused or Toastify Reloaded is hidden in the system tray.

Default shortcuts:

| Action | Shortcut |
|---|---|
| Play / Pause | `Ctrl+Alt+Space` |
| Next track | `Ctrl+Alt+Right` |
| Previous track | `Ctrl+Alt+Left` |
| Volume up | `Ctrl+Alt+Up` |
| Volume down | `Ctrl+Alt+Down` |
| Mute | `Ctrl+Alt+M` |
| Seek +10 seconds | `Ctrl+Alt+Shift+Right` |
| Seek -10 seconds | `Ctrl+Alt+Shift+Left` |
| Show toast | `Ctrl+Alt+T` |

All hotkeys can be customized.

> Windows-reserved combinations or combinations already registered by another application may not be available.

---

## Classic toast, modernized

The classic Toastify notification remains the basis of the popup.

The default minimum geometry stays close to the historical Toastify layout, while Reloaded can now adapt it dynamically to modern metadata and displays.

### Album artwork

The toast can display:

- **Album cover**
- **Toastify Reloaded icon**
- **No image**

When **Album cover** is selected, Toastify Reloaded reads the thumbnail provided by the current Windows media session. If Spotify does not expose artwork, the Toastify icon can be used automatically as fallback.

### Adaptive width

The toast can automatically grow to fit long song titles and artist names.

Example defaults:

```text
Minimum width: 250 px
Maximum width: 600 px
```

If automatic resizing is disabled, the manual Toast Width setting is used instead.

---

### Optional song timeline

The playback timeline is independently configurable.

```text
Show song progress bar:     On / Off
Show song time / duration:  On / Off
```

When enabled, song time is displayed as:

```text
current time / total duration
1:26 / 3:20
```

The progress bar uses the current Windows media-session timeline and updates while the toast is visible.


# Toast themes

Toastify Reloaded v1.3.0 includes **13 built-in presets plus Custom**.

| Theme | Style |
|---|---|
| **Classic Toastify** | Historical gray-to-black Toastify look |
| **Spotify Green** | Dark background with Spotify-style green accents |
| **Midnight Blue** | Deep navy and cool blue |
| **Neon Purple** | Dark background with vivid purple |
| **Cyberpunk** | Blue / purple / hot-pink contrast |
| **Crimson Night** | Black and crimson red |
| **Amber Gold** | Warm dark tones with gold accents |
| **Emerald** | Dark green and emerald |
| **Ocean** | Deep blue and turquoise |
| **Sakura** | Dark violet and soft pink |
| **Arctic** | Cool gray and ice blue |
| **Monochrome** | Black, white and neutral gray |
| **Retro Synthwave** | Purple, magenta and warm neon accents |
| **Custom** | Full manual control |

**Classic Toastify remains the default preset.**

Theme presets affect the visual palette of the toast without removing the existing manual color controls.

In **v1.3.2**, there is no separate Apply button:

```text
Select preset
    ↓
Preset values load into the normal controls
    ↓
Optional manual adjustments
    ↓
Save
```

`Save` is the single persistence action and no success-confirmation popup is shown.

---

# Toast animations

Available animation modes:

- **Fade**
- **Slide**
- **Fade + Slide**
- **None**

### Fade timing

You can configure entry and exit timing independently.

A value of `0 ms` can be used where applicable to disable a fade phase.

### Slide In / Slide Out

Starting with **v1.3.2**, entry and exit slide motion are independent.

Available directions:

- **Up**
- **Down**
- **Left**
- **Right**

Independent controls:

```text
Slide In direction
Slide Out direction
Slide In distance
Slide Out distance
Fade / Enter time
Fade / Exit time
Display time
```

This allows combinations such as:

```text
Enter from Left
Exit toward Down
```

or:

```text
Enter from Bottom
Exit toward Right
```

Legacy animation settings from earlier v1.3 builds are retained as fallback values when migrating an existing configuration.

---

# Positioning & multi-monitor

Toastify Reloaded supports position presets including:

```text
Top Left       Top Center       Top Right
Middle Left    Center           Middle Right
Bottom Left    Bottom Center    Bottom Right
Custom
```

You can also select the target monitor and configure custom offsets / screen coordinates.

This is useful for multi-monitor desktops, gaming setups and secondary displays.

---

# Application themes

The **application theme** is separate from the **toast theme**.

For example:

```text
Application theme: Dark
Toast theme: Sakura
```

or:

```text
Application theme: Light
Toast theme: Classic Toastify
```

Available application modes:

- **Follow Windows**
- **Light**
- **Dark**

This keeps the historical Toastify appearance available while allowing a modern dark interface.

---

# Lyrics Plus

Toastify Reloaded integrates with **Spicetify Lyrics Plus**.

The Reloaded tools can:

- enable Lyrics Plus;
- restore it after Spotify updates;
- remove it;
- preserve it automatically during Compatibility Guard repairs.

Toastify Reloaded does **not** bundle or redistribute song lyrics.

---

# Compatibility Guard

Spotify updates can overwrite Spicetify modifications. Compatibility Guard is designed to detect that change and repair the customization automatically.

When a new Spotify version is detected, Toastify Reloaded can:

1. detect the Spotify version change;
2. close Spotify;
3. optionally update Spicetify;
4. run `spicetify backup apply`;
5. fall back to `spicetify restore backup apply` when needed;
6. ensure Lyrics Plus remains enabled;
7. apply the customization;
8. restart Spotify;
9. remember the successfully repaired Spotify version.

If a brand-new Spotify build is temporarily unsupported by Spicetify, Reloaded avoids repeatedly entering an endless repair loop for the same version.

---

# Automatic updates

Toastify Reloaded can check GitHub Releases for newer versions.

Installed builds use the Windows installer update path, preserving the user's configuration in:

```text
%APPDATA%\ToastifyReloaded\
```

Update behavior can be configured from the Reloaded settings.

---

# Saving settings

Toastify Reloaded uses the main **Save** button as the single persistence action for interface, toast, theme, animation and other settings.

Starting with **v1.3.2**:

- there is no separate **Apply preset** action;
- successful saves do not display a confirmation popup;
- genuine validation or hotkey-registration errors can still display a warning.

---

# Import / export settings

Toastify Reloaded v1.3.0 can export the current configuration to JSON and import it again later.

This is useful for:

- backups;
- moving settings to another PC;
- experimenting with themes;
- restoring hotkey configurations.

---

# Diagnostics

Diagnostic information can include:

- Toastify Reloaded version;
- Windows version;
- display / DPI information;
- monitor information;
- Spotify version;
- Spicetify version;
- Compatibility Guard state;
- active application theme;
- active toast theme;
- animation configuration;
- maintenance settings.

Reports can be copied or exported for troubleshooting.

---

# Privacy

Toastify Reloaded does **not** require:

- your Spotify password;
- your Spotify username;
- a Spotify developer Client ID;
- a Spotify developer Client Secret.

Playback information is obtained from Windows media sessions.

User settings are stored locally.

Network access is mainly used for update checks and components such as Spicetify that require online access.

---

# Compatibility

| Component | Status |
|---|---|
| Windows 11 x64 | Supported |
| Windows 10 x64 | Supported |
| Windows ARM64 | Installer/build available |
| Spotify desktop client | Supported |
| Global hotkeys | Supported |
| Multiple monitors | Supported |
| Light interface | Supported |
| Dark interface | Supported |
| Follow Windows theme | Supported |
| Spicetify / Lyrics Plus | Supported when compatible with the installed Spotify build |

Very new Spotify versions may temporarily require a newer Spicetify release before injected UI modifications can be restored.

---

# Project lineage & credits

Toastify Reloaded exists because of the developers who created and maintained Toastify before this project.

Special credit goes to:

- **[nachmore](https://github.com/nachmore)** — creator / maintainer of the original GitHub Toastify project: [nachmore/toastify](https://github.com/nachmore/toastify)
- **Alessandro Attard Barbini — [@aleab](https://github.com/aleab)** — maintainer of the later Toastify fork and the historical **1.11.x** generation used as a major visual and behavioral reference: [aleab/toastify](https://github.com/aleab/toastify)
- **[Marlius92](https://github.com/Marlius92)** — Toastify Reloaded development and maintenance.

The original authors are **not responsible for Toastify Reloaded**, its new code, support, releases, compatibility changes or future development.

## Project philosophy

Toastify Reloaded is not intended to erase or redesign Toastify.

Its goal is to keep the classic application recognizable while replacing obsolete or broken integrations with implementations that work with current Windows and Spotify versions.

> **Classic Toastify experience. Modern Spotify compatibility.**

---

# Completed roadmap

## Completed before / through v1.3.2

- [x] Historical Toastify-style interface
- [x] Global hotkeys independent of the focused window
- [x] Fade In / Fade Out controls
- [x] Adaptive toast width
- [x] Spotify album artwork in toast
- [x] Toastify icon fallback
- [x] Windows installer
- [x] Automatic Toastify Reloaded updates
- [x] Compatibility Guard
- [x] Automatic Spicetify recovery after Spotify updates
- [x] Lyrics Plus integration
- [x] Built-in colored toast theme presets
- [x] Additional toast animation styles
- [x] Position presets
- [x] Multi-monitor support
- [x] Import / export Toastify Reloaded settings
- [x] Localization infrastructure
- [x] English and Italian interface support
- [x] Additional diagnostics and compatibility reporting
- [x] Automated historical-UI regression checks
- [x] Light / Dark / Follow Windows application themes
- [x] Complete Dark-theme surface/control rendering fix
- [x] Silent Save workflow without success popup
- [x] Theme preset selection without separate Apply action
- [x] Optional song time / duration display
- [x] Live song progress-bar timeline
- [x] Independent Slide In / Slide Out directions
- [x] Independent Slide In / Slide Out distances

---

# Possible future improvements

- [ ] User-created toast presets that can be saved and shared
- [ ] Import / export individual toast themes
- [ ] Additional interface languages
- [ ] More animation styles and easing curves
- [ ] Per-monitor toast profiles
- [ ] More accessibility and keyboard-navigation options
- [ ] Optional signed Windows installer / Authenticode releases
- [ ] Additional diagnostic export formats
- [ ] Expanded automated UI screenshot / visual-regression testing

Suggestions and contributions are welcome through GitHub Issues and Pull Requests.

---

# Reporting a problem

When opening an Issue, useful information includes:

- Toastify Reloaded version
- Spotify version
- Spicetify version
- Windows version
- x64 or ARM64
- whether the problem also happens with the **Classic Toastify** toast theme
- active animation mode and Slide In / Slide Out directions when the issue concerns motion
- whether song progress and song duration are enabled when the issue concerns the toast timeline
- Compatibility Guard status
- screenshots when the issue is visual

Please avoid posting passwords, tokens or other private information.

---

# Build from source

Requirements:

- Windows 10 or Windows 11
- .NET 8 SDK
- Git
- NSIS when building the Windows installer

Clone:

```powershell
git clone https://github.com/Marlius92/Toastify-Reloaded.git
cd Toastify-Reloaded
```

Build:

```powershell
.\scripts\build.ps1
```

Build the x64 installer:

```powershell
.\scripts\build-installer.ps1 -Runtime win-x64
```

---

# Repository structure

```text
Toastify-Reloaded/
├─ .github/
│  └─ workflows/
├─ docs/
├─ installer/
├─ scripts/
├─ src/
│  └─ ToastifyReloaded/
│     ├─ Models/
│     ├─ Native/
│     ├─ Resources/
│     └─ Services/
├─ CHANGELOG.md
├─ CONTRIBUTING.md
├─ LICENSE
├─ NOTICE.md
├─ README.md
├─ SECURITY.md
└─ ToastifyReloaded.sln
```

---

# License

Toastify Reloaded is distributed under the **GNU General Public License v2.0 (GPL-2.0)**.

See [`LICENSE`](LICENSE) and [`NOTICE.md`](NOTICE.md) for licensing and attribution details.

Spotify and related trademarks are property of Spotify AB.

Toastify Reloaded is an independent open-source project and is not affiliated with, sponsored by or endorsed by Spotify AB or by the original Toastify developers.
