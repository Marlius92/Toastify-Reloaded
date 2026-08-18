# macOS feature parity — Toastify Reloaded

Target: **macOS v1.5.0** aligned with Windows v1.3.4 and Linux v1.4.0.

| User-facing capability | Windows 1.3.4 | Linux 1.4.0 | macOS Preview 1 target |
|---|---:|---:|---:|
| Classic Toastify-style UI | Yes | Yes | Yes |
| Light / Dark / Follow System | Yes | Yes | Yes |
| English / Italian UI | Yes | Yes | Yes |
| Global configurable hotkeys | Yes | Yes | Yes |
| Album artwork | Yes | Yes | Yes |
| Toastify icon fallback | Yes | Yes | Yes |
| No-image toast | Yes | Yes | Yes |
| Adaptive toast width | Yes | Yes | Yes |
| Min / max adaptive width | Yes | Yes | Yes |
| Song progress bar | Yes | Yes | Yes |
| Song time / duration | Yes | Yes | Yes |
| 13 presets + Custom | Yes | Yes | Yes |
| Manual colors | Yes | Yes | Yes |
| Font configuration | Yes | Yes | Yes |
| Fade In / Fade Out | Yes | Yes | Yes |
| Slide / Fade + Slide / None | Yes | Yes | Yes |
| Independent slide-in / slide-out | Yes | Yes | Yes |
| Monitor selection | Yes | Yes | Yes |
| Position presets / margins | Yes | Yes | Yes |
| System tray controls | Yes | Yes | Yes |
| Start with session | Yes | Yes | Yes |
| Import / export settings | Yes | Yes | Yes |
| Spicetify / Lyrics Plus | Yes | Yes | Yes |
| Compatibility Guard | Yes | Yes | Yes |
| Update checks | Yes | Yes | Yes |
| Architecture-aware self-update | Yes | Yes | Yes |

## Platform implementation mapping

| Function | Windows | Linux | macOS |
|---|---|---|---|
| Spotify metadata/control | Windows media session | MPRIS / playerctl | Spotify Apple Events / AppleScript |
| Global hotkeys | Windows native registration | X11 / desktop portal backends | SharpHook + macOS Accessibility |
| Start on login | Windows startup integration | XDG autostart | LaunchAgent |
| Settings directory | `%APPDATA%` | XDG config | `~/Library/Application Support` |
| Installer/package | Windows installer | AppImage / DEB / tar.gz | `.app` / DMG / ZIP |
| Update replacement | Windows installer path | package/app replacement | architecture ZIP + app-bundle replacement |

The implementation changes above are platform adapters, not changes to the Toastify Reloaded feature contract.

## Preview 1 acceptance gate

Before promoting Preview 1 to RC1:

- GitHub **macOS Build** must be green;
- x64 and arm64 publish outputs must be valid Mach-O binaries;
- both `.app` bundles must pass `plutil` and `codesign --verify`;
- ZIP and DMG packages must be generated for both architectures;
- the built-in self-test must report `SELF-TEST RESULT: PASS`;
- a real Mac must confirm Spotify Automation permission and metadata/control;
- a real Mac must confirm Accessibility permission and global hotkeys;
- toast artwork/progress/theme/animation behavior must be visually checked;
- Spicetify/Lyrics Plus and Compatibility Guard must be checked on an installed Spotify client;
- the in-app updater must be checked from Preview 1 to a later test build.

No feature additions are required between Preview 1 and stable unless testing exposes a parity gap.
