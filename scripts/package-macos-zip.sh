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
OUT="$ROOT/dist/ToastifyReloaded-macOS-$ARCH.zip"
[[ -d "$APP" ]] || { echo "App bundle missing: $APP" >&2; exit 1; }
rm -f "$OUT"
/usr/bin/ditto -c -k --sequesterRsrc --keepParent "$APP" "$OUT"
echo "Created: $OUT"
