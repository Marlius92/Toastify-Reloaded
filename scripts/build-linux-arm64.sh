#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT/src/ToastifyReloaded.Linux/ToastifyReloaded.Linux.csproj"
OUT="$ROOT/dist/linux-arm64"

rm -rf "$OUT"
mkdir -p "$OUT"

dotnet restore "$PROJECT" -r linux-arm64
dotnet publish "$PROJECT" \
  -c Release \
  -r linux-arm64 \
  --self-contained true \
  -o "$OUT" \
  -p:PublishSingleFile=false \
  -p:PublishTrimmed=false

chmod +x "$OUT/ToastifyReloaded.Linux"

echo "Linux ARM64 publish created at: $OUT"
