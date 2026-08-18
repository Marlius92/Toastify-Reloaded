## Linux Preview

A separate Linux port is now under development using **Avalonia** and Linux
**MPRIS** media controls.

Current preview packages:

- `ToastifyReloaded-Linux-x64.AppImage`
- `toastify-reloaded_1.4.0~preview3_amd64.deb`
- `ToastifyReloaded-Linux-x64.tar.gz`

The first preview supports Spotify playback controls, track-change toasts, album
artwork, adaptive width, toast presets, animations, Spicetify/Lyrics helpers and
custom global hotkeys on X11.

Wayland note: Preview 2 enables Avalonia's native Wayland backend and uses the XDG Global Shortcuts portal for custom global hotkeys when the desktop provides it.

See [`docs/LINUX.md`](docs/LINUX.md) for requirements, installation and current
limitations.


### Linux Preview 3

Preview 3 adds system tray support, runtime IT/EN localization, JSON settings
import/export, an automatic Spotify/Spicetify Compatibility Guard, Linux preview
update checking and initial ARM64 packages.

ARM64 assets:

- `ToastifyReloaded-Linux-arm64.tar.gz`
- `toastify-reloaded_1.4.0~preview3_arm64.deb`
