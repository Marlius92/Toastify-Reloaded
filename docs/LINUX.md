# Toastify Reloaded — Linux Preview

Version line: **1.4.0-preview.1**

The Linux port is intentionally isolated from the stable Windows/WPF project.

## Architecture

- UI: **Avalonia 12.1.1**
- Spotify control/metadata: **MPRIS**, accessed through `playerctl`
- X11 global custom hotkeys: **xbindkeys**
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

The first Linux Preview uses `xbindkeys` for custom global hotkeys.

- **X11:** supported.
- **Wayland:** Spotify/MPRIS control and the application UI work, but custom global
  hotkeys are not guaranteed because Wayland intentionally restricts arbitrary
  global keyboard grabs.

A future preview will use `org.freedesktop.portal.GlobalShortcuts` on desktops
that provide the XDG Global Shortcuts portal.

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

- XDG Global Shortcuts portal backend for Wayland
- Linux-native self-updater
- complete English/Italian localization parity
- complete settings import/export parity
- system tray parity across GNOME/KDE
- Compatibility Guard automatic Spotify-version repair loop
- ARM64 Linux package
- native Wayland backend testing

These are planned before calling the Linux port stable.
