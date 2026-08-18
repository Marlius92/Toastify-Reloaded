#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
APP="$ROOT/dist/linux-x64/ToastifyReloaded.Linux"
LOG="$ROOT/dist/linux-smoke-test.log"

test -x "$APP" || { echo "Executable missing: $APP" >&2; exit 1; }

{
  echo "Toastify Reloaded Linux CI smoke test"
  echo "Kernel: $(uname -a)"
  echo "Architecture: $(uname -m)"
  echo "playerctl: $(command -v playerctl || true)"
  echo "xbindkeys: $(command -v xbindkeys || true)"
  file "$APP"
  echo "--- ldd ---"
  ldd "$APP" || true
} | tee "$LOG"

if ldd "$APP" 2>/dev/null | grep -q "not found"; then
  echo "Unresolved shared-library dependency." | tee -a "$LOG"
  exit 1
fi

set +e
timeout --signal=TERM --kill-after=3s 10s   xvfb-run -a -s "-screen 0 1280x800x24"   env XDG_SESSION_TYPE=x11 DOTNET_EnableDiagnostics=0   "$APP" >>"$LOG" 2>&1
code=$?
set -e

pkill -f xbindkeys >/dev/null 2>&1 || true

if [[ "$code" -ne 124 && "$code" -ne 0 ]]; then
  echo "GUI smoke test failed with exit code $code" | tee -a "$LOG"
  cat "$LOG"
  exit "$code"
fi

if [[ "$code" -eq 124 ]]; then
  echo "PASS: Avalonia app stayed alive for 10 seconds under Xvfb." | tee -a "$LOG"
else
  echo "PASS: Avalonia app exited cleanly." | tee -a "$LOG"
fi
