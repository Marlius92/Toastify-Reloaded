# Toastify Reloaded — Linux

Stable version: **1.4.0**  
Stable tag: **`v1.4.0-linux`**

The Linux port is intentionally isolated from the stable Windows/WPF application and uses Linux-native backends where Windows APIs do not exist.

## Architecture

- UI: **Avalonia 12.1.1**, including opt-in native Wayland backend
- Spotify control/metadata: **MPRIS**, accessed through `playerctl`
- X11 global custom hotkeys: **xbindkeys**
- Wayland global custom hotkeys: **XDG Global Shortcuts Portal** through D-Bus
- Spicetify / Lyrics Plus: existing `spicetify` CLI
- Settings: `~/.config/toastify-reloaded/settings.json`
- Autostart: `~/.config/autostart/io.github.Marlius92.ToastifyReloaded.desktop`

## Linux 1.4.0 features

- Play / Pause, Previous / Next
- Seek ±10 seconds
- Spotify volume and mute controls
- Automatic MPRIS player detection/reconnection
- Track-change toast
- Adaptive and fixed toast geometry
- Album artwork
- Toastify icon fallback
- Optional progress bar
- Optional current time / duration
- Fade, Slide, Fade + Slide and None animation modes
- Independent Slide In / Slide Out direction and distance
- 13 built-in Toastify Reloaded theme presets plus Custom
- Custom toast palette
- Font family and independent title/artist/time sizes
- Light / Dark / Follow system application theme
- Position presets, monitor selection and custom X/Y margins
- X11 and Wayland global shortcuts
- Linux session diagnostics
- System tray controls
- Optional close-to-tray
- English / Italian runtime localization
- JSON settings import/export
- Spicetify Lyrics Plus enable/repair helpers
- Compatibility Guard with failed-version anti-loop behavior
- Package-aware update checks/download/application
- Linux session autostart
- x64 and ARM64 release packages

See `docs/LINUX_PARITY.md` for the Windows/Linux parity matrix.

## Requirements

The Debian package declares:

```text
playerctl
xbindkeys
xdg-desktop-portal
libx11-6
libice6
libsm6
libfontconfig1
```

For AppImage/tar users, install the required host tools and desktop libraries using the package manager for the distribution.

### Ubuntu / Debian

```bash
sudo apt install playerctl xbindkeys xdg-desktop-portal libx11-6 libice6 libsm6 libfontconfig1
```

### Fedora

```bash
sudo dnf install playerctl xbindkeys xdg-desktop-portal libX11 libICE libSM fontconfig
```

### Arch Linux

```bash
sudo pacman -S playerctl xbindkeys xdg-desktop-portal libx11 libice libsm fontconfig
```

## Spotify / MPRIS

`playerctl -l` should list a player whose name starts with `spotify` (or a compatible MPRIS implementation such as `spotifyd`).

Test:

```bash
playerctl --player=spotify status
playerctl --player=spotify metadata title
```

## X11 vs Wayland

Toastify Reloaded selects the hotkey backend from the active session:

- **X11:** `xbindkeys`
- **Wayland:** `org.freedesktop.portal.GlobalShortcuts`

On Wayland, the desktop portal may present a confirmation/configuration dialog the first time Toastify Reloaded binds shortcuts. The portal backend must be provided by the desktop environment.

Avalonia's native Wayland backend is enabled when `WAYLAND_DISPLAY` is available. Set:

```bash
TOASTIFY_DISABLE_NATIVE_WAYLAND=1
```

to force the normal platform-detection/XWayland path.

## System tray

Tray integration uses Avalonia's Linux StatusNotifierItem/AppIndicator support. Availability therefore depends on the desktop environment; some GNOME configurations require an AppIndicator-compatible extension. Close-to-tray is optional and disabled by default.

## Compatibility Guard

When the installed Spotify version changes, Compatibility Guard can run the Spicetify recovery flow:

```bash
spicetify backup apply
```

with fallback:

```bash
spicetify restore backup apply
```

When supported by the Spicetify installation method, Toastify Reloaded can also attempt `spicetify upgrade` before repair. Failed repairs are recorded so the same unsupported Spotify version is not retried indefinitely.

## Settings and import/export

The normal settings file is:

```text
~/.config/toastify-reloaded/settings.json
```

The application can import/export JSON settings through Avalonia's storage provider.

## Update behavior

The stable updater checks GitHub Releases and only accepts later **stable Linux** tags for stable installations.

Channel policy:

- stable → later stable only;
- preview/RC → later preview, RC or stable;
- stable will not auto-install a future preview or RC.

Package-aware application behavior:

- **AppImage:** download, verify SHA-256 digest when GitHub exposes one, replace the running AppImage when possible, restart;
- **`.deb`:** download, verify SHA-256 when available, use the privileged package path when available; otherwise leave the downloaded package for manual installation;
- **tar.gz:** download the matching portable archive without overwriting an unknown custom installation.

## Release packages

### x64

```text
ToastifyReloaded-Linux-x64.AppImage
ToastifyReloaded-Linux-x64.tar.gz
toastify-reloaded_1.4.0_amd64.deb
```

### ARM64

```text
ToastifyReloaded-Linux-arm64.tar.gz
toastify-reloaded_1.4.0_arm64.deb
```

Checksums:

```text
SHA256SUMS.txt
```

## AppImage notes

The AppImage contains Toastify Reloaded and its .NET runtime. Host-side media/shortcut integration still uses Linux services such as `playerctl`, `xbindkeys` (X11) and the desktop portal (Wayland).

The AppDir contains:

```text
AppRun
io.github.Marlius92.ToastifyReloaded.desktop
io.github.Marlius92.ToastifyReloaded.png
usr/bin/toastify-reloaded
usr/lib/toastify-reloaded/ToastifyReloaded.Linux
```

The `usr/bin/toastify-reloaded` wrapper matches the `Exec=toastify-reloaded` desktop entry and launches the bundled application.

## Build locally

Requirements:

- .NET 8 SDK
- curl
- `dpkg-deb` for `.deb`
- common Avalonia Linux libraries

Build x64:

```bash
chmod +x scripts/build-linux.sh
./scripts/build-linux.sh
```

Build ARM64:

```bash
chmod +x scripts/build-linux-arm64.sh
./scripts/build-linux-arm64.sh
```

Build x64 packages:

```bash
chmod +x scripts/package-linux-tar.sh scripts/package-linux-deb.sh scripts/package-linux-appimage.sh
./scripts/package-linux-tar.sh
./scripts/package-linux-deb.sh
./scripts/package-linux-appimage.sh
```

Build ARM64 packages:

```bash
chmod +x scripts/package-linux-arm64-tar.sh scripts/package-linux-arm64-deb.sh
./scripts/package-linux-arm64-tar.sh
./scripts/package-linux-arm64-deb.sh
```

## CI / stable release gate

The normal Linux CI validates:

- x64 build;
- stable headless self-test (`--self-test`);
- GUI startup under virtual X11;
- x64 tar.gz, `.deb` and AppImage packaging;
- Debian content/architecture metadata;
- desktop entry validity;
- AppImage structure;
- ARM64 cross-build and ELF architecture;
- ARM64 tar.gz and `.deb` metadata.

The tag workflow for:

```text
v1.4.0-linux
```

rebuilds all stable assets, generates `SHA256SUMS.txt` and publishes the GitHub Release only after the release package checks pass.

## Platform-specific notes

Functional parity does not mean identical OS implementation. Linux deliberately uses MPRIS, X11/Wayland shortcut backends and StatusNotifierItem/AppIndicator instead of Windows media/session APIs. Desktop-environment behavior can therefore differ in permission dialogs, tray availability and fullscreen detection.
