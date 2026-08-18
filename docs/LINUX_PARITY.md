# Linux / Windows feature parity

Target Windows baseline: **Toastify Reloaded v1.3.4**  
Linux stable: **v1.4.0-linux**

## Core parity

| Feature | Windows | Linux 1.4.0 |
|---|---:|---:|
| Global media hotkeys | Yes | Yes |
| Spotify media session integration | Yes | Yes, MPRIS |
| Track-change toast | Yes | Yes |
| Album artwork | Yes | Yes |
| Toastify icon fallback | Yes | Yes |
| Adaptive toast width | Yes | Yes |
| Progress bar | Yes | Yes |
| Song time / duration | Yes | Yes |
| Light / Dark / system app theme | Yes | Yes |
| 13 built-in toast themes | Yes | Yes |
| Custom toast theme | Yes | Yes |
| Toast colors | Yes | Yes |
| Toast font / font sizes | Yes | Yes |
| Fade / Slide / Fade+Slide / None | Yes | Yes |
| Independent Slide In / Slide Out | Yes | Yes |
| Position corners | Yes | Yes |
| Multi-monitor selection | Yes | Yes |
| Import / Export settings | Yes | Yes |
| Diagnostics | Yes | Yes |
| Italiano / English | Yes | Yes |
| System tray | Yes | Yes |
| Start with session | Yes | Yes |
| Lyrics Plus helper | Yes | Yes |
| Spicetify repair | Yes | Yes |
| Compatibility Guard anti-loop | Yes | Yes |
| Update checking | Yes | Yes |
| Update download/apply | Yes | Yes* |
| x64 package | Yes | Yes |
| ARM64 package | Yes | Yes |

\* Linux update application is package-aware:
- AppImage: downloads, verifies GitHub SHA-256 when supplied, replaces the
  current AppImage and restarts.
- `.deb`: downloads, verifies SHA-256 when supplied and uses `pkexec apt-get`
  when available; otherwise leaves the downloaded package for the user.
- portable tar.gz: downloads the matching archive without overwriting an
  unknown custom installation.

## Linux-specific backends

- X11 global shortcuts: `xbindkeys`.
- Wayland global shortcuts: XDG Global Shortcuts Portal.
- Spotify: MPRIS via `playerctl`.
- Native Wayland: Avalonia Wayland backend when available.
- Tray: StatusNotifierItem/AppIndicator through Avalonia.

## Stable status

Linux v1.4.0 is the first stable Linux line targeting functional parity with
Windows v1.3.4. Future Linux changes can now follow normal patch/minor releases
instead of preview/RC builds.
