#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
RID="${1:-}"
VERSION="${TOASTIFY_MACOS_VERSION:-1.5.0}"
BUILD_VERSION="${TOASTIFY_MACOS_BUILD_VERSION:-1500}"

case "$RID" in
  osx-x64|osx-arm64) ;;
  *)
    echo "Usage: $0 osx-x64|osx-arm64" >&2
    exit 2
    ;;
esac

PUBLISH="$ROOT/dist/macos/$RID/publish"
APP_DIR="$ROOT/dist/macos/$RID/Toastify Reloaded.app"
CONTENTS="$APP_DIR/Contents"
MACOS_DIR="$CONTENTS/MacOS"
RESOURCES="$CONTENTS/Resources"
INFO_TEMPLATE="$ROOT/packaging/macos/Info.plist.template"
ENTITLEMENTS="$ROOT/packaging/macos/ToastifyReloaded.entitlements"
ICON_SOURCE="$ROOT/src/ToastifyReloaded.Mac/Assets/toastify.png"
MAIN_EXECUTABLE="$MACOS_DIR/ToastifyReloaded.Mac"

[[ -x "$PUBLISH/ToastifyReloaded.Mac" ]] || {
  echo "Published executable not found: $PUBLISH/ToastifyReloaded.Mac" >&2
  exit 1
}

# The macOS publish is deliberately single-file. Managed DLLs in Contents/MacOS
# can be interpreted as nested code while the outer bundle is sealed.
if /usr/bin/find "$PUBLISH" -maxdepth 1 -type f -name '*.dll' -print -quit | /usr/bin/grep -q .; then
  echo "ERROR: refusing to package a multi-file managed publish." >&2
  echo "Re-run scripts/build-macos.sh; no managed .dll files may remain." >&2
  /usr/bin/find "$PUBLISH" -maxdepth 1 -type f -name '*.dll' -print >&2
  exit 1
fi

rm -rf "$APP_DIR"
mkdir -p "$MACOS_DIR" "$RESOURCES"
cp -R "$PUBLISH"/. "$MACOS_DIR"/
chmod +x "$MAIN_EXECUTABLE"

sed \
  -e "s/__VERSION__/$VERSION/g" \
  -e "s/__BUILD_VERSION__/$BUILD_VERSION/g" \
  "$INFO_TEMPLATE" > "$CONTENTS/Info.plist"

ICONSET="$ROOT/dist/macos/$RID/ToastifyReloaded.iconset"
rm -rf "$ICONSET"
mkdir -p "$ICONSET"
make_icon() {
  local size="$1"
  local name="$2"
  /usr/bin/sips -z "$size" "$size" "$ICON_SOURCE" --out "$ICONSET/$name" >/dev/null
}
make_icon 16   icon_16x16.png
make_icon 32   icon_16x16@2x.png
make_icon 32   icon_32x32.png
make_icon 64   icon_32x32@2x.png
make_icon 128  icon_128x128.png
make_icon 256  icon_128x128@2x.png
make_icon 256  icon_256x256.png
make_icon 512  icon_256x256@2x.png
make_icon 512  icon_512x512.png
make_icon 1024 icon_512x512@2x.png
/usr/bin/iconutil -c icns "$ICONSET" -o "$RESOURCES/ToastifyReloaded.icns"
rm -rf "$ICONSET"

/usr/bin/plutil -lint "$CONTENTS/Info.plist"
/usr/bin/plutil -lint "$ENTITLEMENTS"

SIGNING_IDENTITY="${MACOS_SIGNING_IDENTITY:--}"
if [[ "$SIGNING_IDENTITY" == "-" ]]; then
  echo "No Developer ID identity configured; applying ad-hoc signatures for CI/preview use."
else
  echo "Signing with: $SIGNING_IDENTITY"
fi

sign_nested_code() {
  local candidate="$1"
  if [[ "$SIGNING_IDENTITY" == "-" ]]; then
    /usr/bin/codesign --force --sign - "$candidate"
  else
    # Apple recommends no app entitlements on library code.
    /usr/bin/codesign --force --timestamp --options runtime --sign "$SIGNING_IDENTITY" "$candidate"
  fi
}

sign_main_executable() {
  if [[ "$SIGNING_IDENTITY" == "-" ]]; then
    /usr/bin/codesign --force --sign - "$MAIN_EXECUTABLE"
  else
    /usr/bin/codesign \
      --force \
      --timestamp \
      --options runtime \
      --entitlements "$ENTITLEMENTS" \
      --sign "$SIGNING_IDENTITY" \
      "$MAIN_EXECUTABLE"
  fi
}

# Sign nested native code from the inside out, main executable next, outer bundle last.
while IFS= read -r -d '' candidate; do
  [[ "$candidate" == "$MAIN_EXECUTABLE" ]] && continue
  if /usr/bin/file "$candidate" | /usr/bin/grep -q 'Mach-O'; then
    echo "Signing nested native component: ${candidate#$APP_DIR/}"
    sign_nested_code "$candidate"
  fi
done < <(/usr/bin/find "$MACOS_DIR" -type f -print0)

echo "Signing main executable: Contents/MacOS/ToastifyReloaded.Mac"
sign_main_executable

echo "Signing application bundle"
if [[ "$SIGNING_IDENTITY" == "-" ]]; then
  /usr/bin/codesign --force --sign - "$APP_DIR"
else
  /usr/bin/codesign \
    --force \
    --timestamp \
    --options runtime \
    --entitlements "$ENTITLEMENTS" \
    --sign "$SIGNING_IDENTITY" \
    "$APP_DIR"
fi

/usr/bin/codesign --verify --verbose=4 "$APP_DIR"

# CI guard: the failure that affected Preview 1 must never return.
if /usr/bin/find "$MACOS_DIR" -maxdepth 1 -type f -name '*.dll' -print -quit | /usr/bin/grep -q .; then
  echo "ERROR: managed DLL found in final Contents/MacOS." >&2
  exit 1
fi

echo "macOS app bundle created and verified: $APP_DIR"
