#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT/src/ToastifyReloaded.Mac/ToastifyReloaded.Mac.csproj"
LOG="$ROOT/dist/macos-self-test.log"
mkdir -p "$ROOT/dist"

{
  echo "Toastify Reloaded macOS CI self-test"
  echo "macOS: $(/usr/bin/sw_vers -productVersion 2>/dev/null || true)"
  echo "Architecture: $(/usr/bin/uname -m)"
  dotnet --info
  echo "--- build ---"
  dotnet restore "$PROJECT"
  dotnet build "$PROJECT" -c Release --no-restore
  echo "--- self-test ---"
  dotnet run --project "$PROJECT" -c Release --no-build -- --self-test
} 2>&1 | tee "$LOG"

/usr/bin/grep -Fq 'SELF-TEST RESULT: PASS' "$LOG"
echo "PASS: macOS self-test"
