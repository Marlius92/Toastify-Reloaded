#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PUBLISH="$ROOT/dist/linux-arm64"
OUT="$ROOT/dist/ToastifyReloaded-Linux-arm64.tar.gz"

if [[ ! -x "$PUBLISH/ToastifyReloaded.Linux" ]]; then
  echo "Run scripts/build-linux-arm64.sh first." >&2
  exit 1
fi

tar -C "$PUBLISH" -czf "$OUT" .
echo "Created: $OUT"
