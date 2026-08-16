# Classic UI reference — Toastify 1.11.2

Toastify Reloaded v1.2.1 uses the historical Toastify 1.11.2 settings window and toast popup as the visual compatibility target.

## Invariants

### Settings window

- Fixed WPF window: `580 × 570`.
- `ResizeMode=NoResize`.
- Native Windows/WPF control chrome; no Reloaded dark theme.
- Historical tabs in order: `General`, `Hotkeys`, `Toast`, `Advanced`.
- One additional tab is permitted: `Reloaded`.
- `Save`: `47 × 23`, right/top margin `0,32,90,0`.
- `Default` split-button: `73 × 23`, right/top margin `0,32,10,0`.
- Historical 120 × 120 Toastify/Spotify logo in the bottom-right of General.

### Toast popup

- `250 × 70`.
- Transparent borderless topmost WPF window.
- Border `#FF292929`, thickness 1, corner radius 4.
- Vertical gradient `#FF555555` to `#FF151515`.
- First column 70 px, 60 × 60 artwork.
- Content margin `15,15,0,4`.
- Top title 16 px white; bottom title 12 px `#FFF0F0F0`.
- Progress background `#FF333333`; foreground `#FFA0A0A0`.

## Reloaded additions

Lyrics Plus, Compatibility Guard, self-update and diagnostics live in the fifth `Reloaded` tab. They must not alter the visual structure of the four historical tabs or the toast popup.

## Verification

Structural checks compare XAML dimensions, tab order, key margins and popup geometry. Final visual parity must also be checked on Windows at the same DPI/theme as the historical application, because native WPF system controls inherit Windows theme metrics.
