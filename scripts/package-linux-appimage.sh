#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PUBLISH="$ROOT/dist/linux-x64"
APPDIR="$ROOT/dist/ToastifyReloaded.AppDir"
OUT="$ROOT/dist/ToastifyReloaded-Linux-x64.AppImage"
APPIMAGETOOL="$ROOT/dist/appimagetool-x86_64.AppImage"

if [[ ! -x "$PUBLISH/ToastifyReloaded.Linux" ]]; then
  echo "Run scripts/build-linux.sh first." >&2
  exit 1
fi

rm -rf "$APPDIR" "$OUT"
mkdir -p \
  "$APPDIR/usr/lib/toastify-reloaded" \
  "$APPDIR/usr/bin" \
  "$APPDIR/usr/share/applications" \
  "$APPDIR/usr/share/icons/hicolor/256x256/apps"

cp -a "$PUBLISH/." "$APPDIR/usr/lib/toastify-reloaded/"
cp "$ROOT/packaging/linux/io.github.Marlius92.ToastifyReloaded.desktop" \
   "$APPDIR/usr/share/applications/"
cp "$ROOT/packaging/linux/io.github.Marlius92.ToastifyReloaded.png" \
   "$APPDIR/usr/share/icons/hicolor/256x256/apps/"

cp "$ROOT/packaging/linux/io.github.Marlius92.ToastifyReloaded.desktop" \
   "$APPDIR/io.github.Marlius92.ToastifyReloaded.desktop"
cp "$ROOT/packaging/linux/io.github.Marlius92.ToastifyReloaded.png" \
   "$APPDIR/io.github.Marlius92.ToastifyReloaded.png"

cat > "$APPDIR/AppRun" <<'EOF'
#!/usr/bin/env bash
HERE="$(dirname "$(readlink -f "$0")")"
exec "$HERE/usr/lib/toastify-reloaded/ToastifyReloaded.Linux" "$@"
EOF
chmod +x "$APPDIR/AppRun"

if [[ ! -x "$APPIMAGETOOL" ]]; then
  curl -L \
    https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage \
    -o "$APPIMAGETOOL"
  chmod +x "$APPIMAGETOOL"
fi

ARCH=x86_64 APPIMAGE_EXTRACT_AND_RUN=1 "$APPIMAGETOOL" "$APPDIR" "$OUT"
chmod +x "$OUT"

echo "Created: $OUT"
echo "Note: playerctl and xbindkeys are external runtime requirements for the preview."
