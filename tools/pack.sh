#!/usr/bin/env bash
# Build + pack the Runbook plugin (Linux/macOS CI).
#
# Prerequisites:
#   - .NET 8 SDK
#   - logiplugintool (from Logi Actions SDK) — optional
#
# Usage: ./tools/pack.sh

set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
SRC="$ROOT/src/Runbook.LogiPlugin"
WIN="$ROOT/win"

echo "--- dotnet publish ---"
dotnet publish "$SRC" \
    --configuration Release \
    --output "$WIN" \
    --self-contained false

if command -v logiplugintool &>/dev/null; then
    echo "--- logiplugintool pack ---"
    logiplugintool pack "$ROOT"
    echo "--- logiplugintool verify ---"
    LPLUG4=$(find "$ROOT" -maxdepth 1 -name '*.lplug4' | head -1)
    if [ -n "$LPLUG4" ]; then
        logiplugintool verify "$LPLUG4"
    fi
else
    echo "WARN: logiplugintool not found. Skipping pack/verify."
fi

echo "Done."
