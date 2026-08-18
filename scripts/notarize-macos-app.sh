#!/usr/bin/env bash
set -euo pipefail

APP="${1:-}"
[[ -n "$APP" && -d "$APP" ]] || {
  echo "Usage: $0 '/path/to/Toastify Reloaded.app'" >&2
  exit 2
}

if [[ "${MACOS_SIGNING_IDENTITY:--}" == "-" ]]; then
  echo "Skipping notarization: app is ad-hoc signed."
  exit 0
fi

if [[ -z "${APPLE_ID:-}" || -z "${APPLE_TEAM_ID:-}" || -z "${APPLE_APP_SPECIFIC_PASSWORD:-}" ]]; then
  echo "Skipping notarization: Apple notarization credentials are not configured."
  exit 0
fi

TMP_ZIP="$RUNNER_TEMP/toastify-notarization-$(/usr/bin/uuidgen).zip"
/usr/bin/ditto -c -k --sequesterRsrc --keepParent "$APP" "$TMP_ZIP"

/usr/bin/xcrun notarytool submit "$TMP_ZIP" \
  --apple-id "$APPLE_ID" \
  --team-id "$APPLE_TEAM_ID" \
  --password "$APPLE_APP_SPECIFIC_PASSWORD" \
  --wait

/usr/bin/xcrun stapler staple "$APP"
/usr/bin/xcrun stapler validate "$APP"
rm -f "$TMP_ZIP"
echo "Notarization complete: $APP"
