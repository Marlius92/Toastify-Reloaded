#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
VERSION="${1:-1.4.0~preview1}"
PUBLISH="$ROOT/dist/linux-x64"
PKGROOT="$ROOT/dist/deb-root"
OUT="$ROOT/dist/toastify-reloaded_${VERSION}_amd64.deb"

if [[ ! -x "$PUBLISH/ToastifyReloaded.Linux" ]]; then
  echo "Run scripts/build-linux.sh first." >&2
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
Architecture: amd64
Maintainer: Toastify Reloaded contributors
Depends: playerctl, xbindkeys, libx11-6, libice6, libsm6, libfontconfig1
Description: Toastify Reloaded Linux Preview
 Spotify toast notifications, MPRIS controls, Spicetify/Lyrics helpers
 and X11 global hotkeys.
EOF

cat > "$PKGROOT/DEBIAN/postinst" <<'EOF'
#!/usr/bin/env bash
set -e
update-desktop-database >/dev/null 2>&1 || true
gtk-update-icon-cache -q /usr/share/icons/hicolor >/dev/null 2>&1 || true
exit 0
EOF
chmod 755 "$PKGROOT/DEBIAN/postinst"

cat > "$PKGROOT/DEBIAN/postrm" <<'EOF'
#!/usr/bin/env bash
set -e
update-desktop-database >/dev/null 2>&1 || true
gtk-update-icon-cache -q /usr/share/icons/hicolor >/dev/null 2>&1 || true
exit 0
EOF
chmod 755 "$PKGROOT/DEBIAN/postrm"

dpkg-deb --build --root-owner-group "$PKGROOT" "$OUT"
echo "Created: $OUT"
