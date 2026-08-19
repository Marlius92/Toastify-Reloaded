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
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=false \
  -p:PublishTrimmed=false \
  -p:DebugType=None \
  -p:DebugSymbols=false

chmod +x "$OUT/ToastifyReloaded.Mac"

echo "Published files:"
/usr/bin/find "$OUT" -maxdepth 1 -type f -print | /usr/bin/sort

# A macOS single-file publish must not leave managed .NET assemblies next to the app host.
if /usr/bin/find "$OUT" -maxdepth 1 -type f -name '*.dll' -print -quit | /usr/bin/grep -q .; then
  echo "ERROR: managed DLLs remain in macOS single-file publish:" >&2
  /usr/bin/find "$OUT" -maxdepth 1 -type f -name '*.dll' -print >&2
  exit 1
fi

echo "macOS single-file publish created at: $OUT"
