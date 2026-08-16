# Security

## Reporting a vulnerability

Open a private security advisory in GitHub when possible. Do not publish tokens, passwords, personal paths, or sensitive diagnostic data in a public issue.

## Design choices

- No Spotify password is requested or stored.
- No Spotify Web API client secret is required.
- Playback control uses Windows media-session APIs.
- Settings are stored locally in `%APPDATA%\ToastifyModern\settings.json`.
- Lyrics setup invokes the locally installed Spicetify command line.
- The program does not disable Windows SmartScreen or security features.
