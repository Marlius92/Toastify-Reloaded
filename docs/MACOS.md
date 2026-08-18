# Toastify Reloaded for macOS

## Current target

**Fast-track Preview 1:** `v1.5.0-macos-preview.1`

The macOS port is intentionally based on the same Avalonia UI and configuration model used by the completed Linux port. The goal is not to redesign Toastify Reloaded: all normal user-facing settings and behavior remain aligned, while only operating-system backends are replaced where macOS requires a different implementation.

## Architectures and packages

The CI publishes both supported 64-bit Mac architectures:

- Apple Silicon: `osx-arm64`
- Intel: `osx-x64`

Preview/release artifacts:

- `ToastifyReloaded-macOS-arm64.dmg`
- `ToastifyReloaded-macOS-arm64.zip`
- `ToastifyReloaded-macOS-x64.dmg`
- `ToastifyReloaded-macOS-x64.zip`
- `SHA256SUMS-macOS.txt`

The ZIP packages are also the assets used by the in-app architecture-aware updater.

## Feature contract

Preview 1 carries the completed desktop feature set across to macOS:

- classic Toastify-style notification;
- album artwork with Toastify icon fallback and no-image mode;
- adaptive toast width and manual width controls;
- optional playback progress and song time / duration;
- 13 built-in toast presets plus Custom;
- full manual toast colors;
- font family and title / artist / time sizes;
- Fade, Slide, Fade + Slide and None;
- independent Slide In / Slide Out direction and distance;
- position presets, monitor selection and margins;
- Light, Dark and Follow System application themes;
- English and Italian interface;
- settings import/export;
- system tray controls;
- global configurable hotkeys;
- start at login;
- Spicetify / Lyrics Plus tools;
- Compatibility Guard after Spotify updates;
- automatic GitHub update checks and optional automatic installation.

Settings are stored at:

```text
~/Library/Application Support/ToastifyReloaded/settings.json
```

The macOS importer also accepts the Linux v1.4 settings JSON. Linux-specific X11/Wayland hotkey choices are mapped to the single macOS global-hotkey backend; all common toast/theme/hotkey values are preserved.

## Platform backends

### Spotify playback and metadata

macOS uses AppleScript through the system `osascript` executable to read Spotify metadata and invoke playback controls.

On first use, macOS can request permission for Toastify Reloaded to automate Spotify. The app bundle includes `NSAppleEventsUsageDescription`, and Developer-ID/hardened-runtime builds include the Apple Events entitlement.

### Global hotkeys

Global hotkeys use SharpHook/libuiohook. macOS requires Accessibility permission for the process before a global keyboard hook can operate.

If hotkeys are not active, enable Toastify Reloaded in:

```text
System Settings
→ Privacy & Security
→ Accessibility
```

Then return to Toastify Reloaded and press **Save** again.

### Spicetify and Lyrics Plus

The macOS Spicetify integration configures the normal Spotify resources path:

```text
/Applications/Spotify.app/Contents/Resources
```

Compatibility Guard retains the same high-level behavior as Linux: detect a Spotify version change, repair/reapply Spicetify, retain Lyrics Plus when enabled, and remember repair attempts to avoid loops.

## Signing and Gatekeeper

The packaging pipeline has two modes:

1. **Preview / no Apple credentials:** CI creates an ad-hoc signed `.app`, ZIP and DMG. This is sufficient for build validation, but a downloaded public build can still trigger Gatekeeper warnings.
2. **Developer ID configured:** CI can import a Developer ID Application certificate, use Hardened Runtime with the minimum required entitlements, and optionally notarize/staple the app when Apple notarization credentials are also configured.

The signing script explicitly signs nested Mach-O files before signing the `.app` bundle and does not use `codesign --deep`.

### Optional GitHub Actions secrets

For Developer ID signing:

```text
APPLE_CERTIFICATE_P12_BASE64
APPLE_CERTIFICATE_PASSWORD
APPLE_KEYCHAIN_PASSWORD
MACOS_SIGNING_IDENTITY    (optional if the certificate name can be discovered)
```

For notarization:

```text
APPLE_ID
APPLE_TEAM_ID
APPLE_APP_SPECIFIC_PASSWORD
```

These secrets are not required for Preview 1 CI compilation or packaging.

## Build from source

On macOS with the .NET 8 SDK:

```bash
./scripts/test-macos-ci.sh
./scripts/build-macos.sh osx-arm64
./scripts/package-macos-app.sh osx-arm64
./scripts/package-macos-zip.sh osx-arm64
./scripts/package-macos-dmg.sh osx-arm64
```

For Intel replace `osx-arm64` with `osx-x64`.

## Fast-track release path

```text
v1.5.0-macos-preview.1
        ↓
CI compile/package validation
        ↓
real Mac + Spotify permission/runtime validation
        ↓
fix only confirmed blockers
        ↓
v1.5.0-macos-rc.1
        ↓
final release gate
        ↓
v1.5.0-macos
```

Preview 1 is deliberately feature-complete. The preview/RC stages are for validating platform integration, packaging, permissions and updater behavior rather than adding a second feature-development cycle.
