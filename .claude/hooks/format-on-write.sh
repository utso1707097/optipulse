#!/usr/bin/env bash
# PostToolUse(Write|Edit): format the edited file by type. Silent no-op if the
# formatter is missing. Never blocks (always exits 0). Adopted/adapted from the
# .NET + Claude-Code best-practice kits (.kits/), generalized across OptiPulse's
# three stacks (.NET / React / Flutter).
set -uo pipefail

payload="$(cat)"
file="$(printf '%s' "$payload" | python3 -c 'import json,sys;print(json.load(sys.stdin).get("tool_input",{}).get("file_path",""))' 2>/dev/null)"
[ -z "$file" ] || [ ! -f "$file" ] && exit 0

case "$file" in
  *.cs)
    command -v dotnet >/dev/null 2>&1 && dotnet format --include "$file" >/dev/null 2>&1 || true
    ;;
  *.ts|*.tsx|*.js|*.jsx|*.json|*.css|*.scss|*.html|*.md)
    if command -v npx >/dev/null 2>&1; then npx --no-install prettier --write "$file" >/dev/null 2>&1 || true; fi
    ;;
  *.dart)
    command -v dart >/dev/null 2>&1 && dart format "$file" >/dev/null 2>&1 || true
    ;;
esac
exit 0
