#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT/src/ToastifyReloaded.Mac/ToastifyReloaded.Mac.csproj"
RID="${1:-}"

case "$RID" in
  osx-x64|osx-arm64) ;;
  *)
    echo "Usage: $0 osx-x64|osx-arm64" >&2
    exit 2
    ;;
esac

OUT="$ROOT/dist/macos/$RID/publish"
rm -rf "$OUT"
mkdir -p "$OUT"

dotnet restore "$PROJECT" -r "$RID"
dotnet publish "$PROJECT" \
  -c Release \
  -r "$RID" \
  --self-contained true \
  -o "$OUT" \
  -p:PublishSingleFile=false \
  -p:PublishTrimmed=false

chmod +x "$OUT/ToastifyReloaded.Mac"
echo "macOS publish created at: $OUT"
