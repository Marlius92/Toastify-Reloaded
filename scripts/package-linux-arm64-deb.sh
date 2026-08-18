#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
VERSION="${1:-1.4.0}"
PUBLISH="$ROOT/dist/linux-arm64"
PKGROOT="$ROOT/dist/deb-arm64-root"
OUT="$ROOT/dist/toastify-reloaded_${VERSION}_arm64.deb"

if [[ ! -x "$PUBLISH/ToastifyReloaded.Linux" ]]; then
  echo "Run scripts/build-linux-arm64.sh first." >&2
  exit 1
fi

rm -rf "$PKGROOT"
mkdir -p \
  "$PKGROOT/DEBIAN" \
  "$PKGROOT/opt/toastify-reloaded" \
  "$PKGROOT/usr/bin" \
  "$PKGROOT/usr/share/applications" \
  "$PKGROOT/usr/share/icons/hicolor/256x256/apps"

cp -a "$PUBLISH/." "$PKGROOT/opt/toastify-reloaded/"
cp "$ROOT/packaging/linux/io.github.Marlius92.ToastifyReloaded.desktop" \
   "$PKGROOT/usr/share/applications/"
cp "$ROOT/packaging/linux/io.github.Marlius92.ToastifyReloaded.png" \
   "$PKGROOT/usr/share/icons/hicolor/256x256/apps/"

cat > "$PKGROOT/usr/bin/toastify-reloaded" <<'EOF'
#!/usr/bin/env bash
exec /opt/toastify-reloaded/ToastifyReloaded.Linux "$@"
EOF
chmod +x "$PKGROOT/usr/bin/toastify-reloaded"

cat > "$PKGROOT/DEBIAN/control" <<EOF
Package: toastify-reloaded
Version: $VERSION
Section: sound
Priority: optional
Architecture: arm64
Maintainer: Toastify Reloaded contributors
Depends: playerctl, xbindkeys, xdg-desktop-portal, libx11-6, libice6, libsm6, libfontconfig1
Description: Toastify Reloaded for Linux ARM64
 Spotify toast notifications, MPRIS controls, Spicetify/Lyrics helpers
 and X11/Wayland global hotkeys.
EOF

dpkg-deb --build --root-owner-group "$PKGROOT" "$OUT"
echo "Created: $OUT"
