# Toastify Reloaded v1.1.0 - Auto Update & Compatibility Guard

## Purpose

Spotify updates can replace files modified by Spicetify. Toastify Reloaded v1.1.0 treats that as a recoverable compatibility event instead of requiring the user to manually re-run Spicetify every time.

## Spotify version detection

The app tries, in order:

1. the running `Spotify` process and its executable file version;
2. `%APPDATA%\Spotify\Spotify.exe`;
3. `%LOCALAPPDATA%\Spotify\Spotify.exe`;
4. the installed `SpotifyAB.SpotifyMusic` AppX package version.

The first version discovered becomes the baseline. Future version changes trigger the Compatibility Guard.

## Recovery flow

When automatic repair is enabled and Spotify changes version:

```text
Spotify version changed
        |
        v
Stop Spotify
        |
        +--> optional: spicetify upgrade
        |
        v
spicetify backup apply
        |
        +--> failed? --> spicetify restore backup apply
        |
        v
Ensure lyrics-plus is configured
        |
        v
spicetify apply
        |
        v
spicetify auto
        |
        v
Record new compatible Spotify version
```

A failed repair is marked for that Spotify version. The periodic checker will not keep closing Spotify and retrying the same failed repair. A manual **Ripara Spotify / Lyrics ora** action bypasses that guard.

## Toastify Reloaded self-update

The app calls the GitHub Latest Release API and compares the current assembly version with the release tag. It expects one of these assets:

- `ToastifyReloaded-win-x64.zip`
- `ToastifyReloaded-win-arm64.zip`

The asset is downloaded and extracted under `%TEMP%`. An updater PowerShell process waits for Toastify Reloaded to exit, copies the new files into the current portable installation folder and starts the new executable.

Settings are stored separately in `%APPDATA%\ToastifyReloaded` and survive application updates.

## Safety decisions

- No update is installed if the expected architecture-specific release asset is missing.
- A GitHub network error does not affect Spotify controls.
- A failed Spicetify repair is not considered successful and does not overwrite the last known compatible Spotify version.
- `spicetify upgrade` is non-fatal because package-managed Spicetify installations may not support self-upgrade.
- The user can disable each automatic behavior independently.
