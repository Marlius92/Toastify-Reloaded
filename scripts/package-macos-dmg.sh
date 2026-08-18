#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
RID="${1:-}"

case "$RID" in
  osx-x64) ARCH=x64 ;;
  osx-arm64) ARCH=arm64 ;;
  *)
    echo "Usage: $0 osx-x64|osx-arm64" >&2
    exit 2
    ;;
esac

APP="$ROOT/dist/macos/$RID/Toastify Reloaded.app"
OUT="$ROOT/dist/ToastifyReloaded-macOS-$ARCH.dmg"
STAGE="$ROOT/dist/macos/$RID/dmg-stage"
[[ -d "$APP" ]] || { echo "App bundle missing: $APP" >&2; exit 1; }

rm -rf "$STAGE" "$OUT"
mkdir -p "$STAGE"
/usr/bin/ditto "$APP" "$STAGE/Toastify Reloaded.app"
/bin/ln -s /Applications "$STAGE/Applications"

/usr/bin/hdiutil create \
  -volname "Toastify Reloaded" \
  -srcfolder "$STAGE" \
  -ov \
  -format UDZO \
  "$OUT" >/dev/null

rm -rf "$STAGE"
echo "Created: $OUT"
