## Linux Preview

A separate Linux port is now under development using **Avalonia** and Linux
**MPRIS** media controls.

Current preview packages:

- `ToastifyReloaded-Linux-x64.AppImage`
- `toastify-reloaded_1.4.0~preview1_amd64.deb`
- `ToastifyReloaded-Linux-x64.tar.gz`

The first preview supports Spotify playback controls, track-change toasts, album
artwork, adaptive width, toast presets, animations, Spicetify/Lyrics helpers and
custom global hotkeys on X11.

Wayland note: Spotify/MPRIS control works, but custom global hotkeys are still
being migrated to the XDG Global Shortcuts portal.

See [`docs/LINUX.md`](docs/LINUX.md) for requirements, installation and current
limitations.
