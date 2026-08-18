#!/usr/bin/env bash
set -euo pipefail

# Optional GitHub Actions setup. When secrets are absent, packaging falls back
# to ad-hoc signing so Preview builds can still be produced and tested.
if [[ -z "${APPLE_CERTIFICATE_P12_BASE64:-}" || -z "${APPLE_CERTIFICATE_PASSWORD:-}" ]]; then
  echo "Apple Developer certificate not configured; using ad-hoc signing."
  if [[ -n "${GITHUB_ENV:-}" ]]; then
    echo 'MACOS_SIGNING_IDENTITY=-' >> "$GITHUB_ENV"
  fi
  exit 0
fi

KEYCHAIN_PASSWORD="${APPLE_KEYCHAIN_PASSWORD:-toastify-ci-keychain}"
KEYCHAIN="$RUNNER_TEMP/toastify-signing.keychain-db"
CERT="$RUNNER_TEMP/toastify-developer-id.p12"

printf '%s' "$APPLE_CERTIFICATE_P12_BASE64" | /usr/bin/base64 -D > "$CERT"
/usr/bin/security create-keychain -p "$KEYCHAIN_PASSWORD" "$KEYCHAIN"
/usr/bin/security set-keychain-settings -lut 21600 "$KEYCHAIN"
/usr/bin/security unlock-keychain -p "$KEYCHAIN_PASSWORD" "$KEYCHAIN"
/usr/bin/security import "$CERT" \
  -k "$KEYCHAIN" \
  -P "$APPLE_CERTIFICATE_PASSWORD" \
  -T /usr/bin/codesign \
  -T /usr/bin/security
/usr/bin/security set-key-partition-list \
  -S apple-tool:,apple:,codesign: \
  -s \
  -k "$KEYCHAIN_PASSWORD" \
  "$KEYCHAIN" >/dev/null
/usr/bin/security list-keychains -d user -s "$KEYCHAIN"

IDENTITY="${MACOS_SIGNING_IDENTITY:-}"
if [[ -z "$IDENTITY" ]]; then
  IDENTITY="$(/usr/bin/security find-identity -v -p codesigning "$KEYCHAIN" \
    | /usr/bin/awk -F'"' '/Developer ID Application/ { print $2; exit }')"
fi

if [[ -z "$IDENTITY" ]]; then
  echo "Developer ID Application identity not found after certificate import." >&2
  exit 1
fi

echo "Developer ID signing identity loaded."
if [[ -n "${GITHUB_ENV:-}" ]]; then
  echo "MACOS_SIGNING_IDENTITY=$IDENTITY" >> "$GITHUB_ENV"
  echo "MACOS_KEYCHAIN_PATH=$KEYCHAIN" >> "$GITHUB_ENV"
else
  export MACOS_SIGNING_IDENTITY="$IDENTITY"
fi
