# Toastify Reloaded — Linux Preview

Version line: **1.4.0-rc.1**

The Linux port is intentionally isolated from the stable Windows/WPF project.

## Architecture

- UI: **Avalonia 12.1.1**, including opt-in native Wayland backend
- Spotify control/metadata: **MPRIS**, accessed through `playerctl`
- X11 global custom hotkeys: **xbindkeys**
- Wayland global custom hotkeys: **XDG Global Shortcuts Portal** via D-Bus
- Spicetify / Lyrics Plus: existing `spicetify` CLI
- Settings: `~/.config/toastify-reloaded/settings.json`
- Autostart: `~/.config/autostart/io.github.Marlius92.ToastifyReloaded.desktop`

## Current Preview features

- Play / Pause
- Previous / Next
- Seek ±10 seconds
- Spotify volume ±5%
- X11 custom global hotkeys
- Track-change toast
- Adaptive toast width
- Album artwork
- Toastify icon fallback
- Optional progress bar
- Optional current time / duration
- Fade, Slide and Fade + Slide animation
- Independent Slide In / Slide Out direction and distance
- 13 Toastify Reloaded theme presets
- Light / Dark / Follow system application theme
- Linux session diagnostics
- Spicetify Lyrics Plus enable
- Spicetify repair (`backup apply`, fallback `restore backup apply`)
- Linux session autostart
- `.deb`, AppImage and tar.gz packaging

## Requirements

Avalonia desktop Linux requires common X11 runtime libraries. The Debian package
declares the following dependencies:

```text
playerctl
xbindkeys
libx11-6
libice6
libsm6
libfontconfig1
```

For AppImage/tar users, install `playerctl` and `xbindkeys` separately.

### Ubuntu / Debian

```bash
sudo apt install playerctl xbindkeys libx11-6 libice6 libsm6 libfontconfig1
```

### Fedora

```bash
sudo dnf install playerctl xbindkeys libX11 libICE libSM fontconfig
```

### Arch Linux

```bash
sudo pacman -S playerctl xbindkeys libx11 libice libsm fontconfig
```

## Spotify

`playerctl -l` should list a player whose name starts with `spotify`.

Test:

```bash
playerctl --player=spotify status
playerctl --player=spotify metadata title
```

## X11 vs Wayland

Preview 2 automatically selects the hotkey backend:

- **X11:** `xbindkeys`.
- **Wayland:** `org.freedesktop.portal.GlobalShortcuts`.

On Wayland, the desktop portal may present a confirmation/configuration dialog
the first time Toastify Reloaded binds shortcuts. The portal backend must be
provided by the desktop environment.

Avalonia 12.1's native Wayland backend is enabled automatically when
`WAYLAND_DISPLAY` is present. Set `TOASTIFY_DISABLE_NATIVE_WAYLAND=1` to force
the normal platform-detection/XWayland path.

## AppImage note

The AppImage contains Toastify Reloaded and its .NET runtime, but currently
expects `playerctl` and `xbindkeys` on the host.

## Build locally

Requirements:

- .NET 8 SDK
- curl
- dpkg-deb (for `.deb`)
- common Avalonia Linux libraries

```bash
chmod +x scripts/build-linux.sh
./scripts/build-linux.sh
```

Build `.deb`:

```bash
chmod +x scripts/package-linux-deb.sh
./scripts/package-linux-deb.sh
```

Build AppImage:

```bash
chmod +x scripts/package-linux-appimage.sh
./scripts/package-linux-appimage.sh
```

## Preview limitations

Not yet at Windows v1.3.4 parity:

- Linux-native self-updater
- complete English/Italian localization parity
- complete settings import/export parity
- system tray parity across GNOME/KDE
- Compatibility Guard automatic Spotify-version repair loop
- ARM64 Linux package
- native Wayland backend testing

These are planned before calling the Linux port stable.


## Preview 3 additions

Preview 3 expands Linux parity substantially:

- Avalonia system tray integration (`TrayIcon`) with Open, Play/Pause, Next,
  Previous and Exit actions.
- Optional close-to-tray behavior.
- Runtime Italian / English localization.
- JSON settings import/export using Avalonia `StorageProvider`.
- Linux Compatibility Guard:
  - detects the installed Spotify version where possible;
  - records version changes;
  - can automatically run the Spicetify post-update repair flow;
  - avoids repeatedly retrying the same failed Spotify version.
- Automatic Linux preview update checks using the GitHub Releases REST API.
- Linux ARM64 self-contained builds:
  - `ToastifyReloaded-Linux-arm64.tar.gz`
  - `toastify-reloaded_1.4.0~preview3_arm64.deb`

### Tray compatibility

Avalonia tray icons work on Linux desktops that expose StatusNotifierItem or
AppIndicator support. Some GNOME configurations require an AppIndicator
extension. Close-to-tray is therefore disabled by default.

### Compatibility Guard

The repair flow follows Spicetify's documented post-Spotify-update workflow:

```bash
spicetify backup apply
```

with fallback:

```bash
spicetify restore backup apply
```

When supported by the installation method, Toastify Reloaded also attempts
`spicetify upgrade` before the repair. A failed `upgrade` command does not abort
the repair because package-manager Spicetify installations can legitimately
reject that command.

### ARM64

ARM64 packages are cross-published with the .NET `linux-arm64` runtime
identifier. GitHub Actions validates the resulting ELF architecture and package
metadata. The ARM64 build is not GUI-smoke-tested on the x64 GitHub runner.


## Preview 4 — Feature Parity Candidate

Preview 4 closes the main remaining gaps with Windows v1.3.4:

- `Toast > Colors & Font`;
- Custom toast palette;
- font family and independent title/artist/time sizes;
- `Toast > Position`;
- primary or explicit monitor selection;
- top-left, top-right, bottom-left and bottom-right placement;
- configurable X/Y margins;
- GitHub release asset discovery;
- package-aware update download/application;
- SHA-256 verification when GitHub provides an asset digest.

See `docs/LINUX_PARITY.md` for the parity matrix.


## RC1 release engineering

RC1 freezes the feature set introduced by Preview 4.

No new product features should be added between RC1 and stable unless they fix a
release-blocking defect.

RC1 adds:

- a headless executable self-test (`--self-test`);
- settings save/load/import/export round-trip tests;
- release-channel ordering tests:
  `preview < rc < stable`;
- package archive content validation;
- stricter x64 and ARM64 release-asset validation;
- an updater that can move from RC builds to the final
  `v1.4.0-linux` stable release.

Stable target:

```text
v1.4.0-linux
```
