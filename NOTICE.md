# Notices and third-party names

Toastify Reloaded is an independent Windows companion for Spotify. It is not affiliated with, endorsed by, or sponsored by Spotify AB.

## Historical Toastify 1.11.2 interface

Toastify Reloaded v1.2.1 intentionally uses the historical Toastify 1.11.2 settings/toast visual structure as the compatibility baseline. The original Toastify project by Alessandro Attard Barbini and contributors was released under the GNU GPL v2.

The Reloaded project keeps the modern Spotify-session, updater, Spicetify repair and installer implementation separate from the historical UI layer, while preserving the original visible geometry and the classic Toastify logo resource for UI compatibility.

Historical reference project: `aleab/toastify`.

`src/ToastifyReloaded/Resources/SpotifyToastifyLogo.png` and the classic UI lineage are therefore distributed under the repository's GPL-v2 terms. Toastify and Spotify names/marks remain the property of their respective owners.

## Extended WPF Toolkit

The historical Toastify settings window used Extended WPF Toolkit controls. Toastify Reloaded targets modern .NET/WPF and references the current `Extended.Wpf.Toolkit` NuGet package declared by the project file. Its own license and notices remain those of its publisher.

## Spicetify / Lyrics Plus

The optional Lyrics integration calls the user's locally installed **Spicetify** CLI and enables its `lyrics-plus` Custom App. This repository does not redistribute Spicetify, Lyrics Plus, Spotify binaries, or song lyrics.
