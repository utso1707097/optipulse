#!/usr/bin/env bash
# Anti-pattern gate (Constitution v2.1.0 — Adopted Toolchain & Practice Baselines).
# Adapted from .kits/dotnet-claude-kit/hooks/pre-commit-antipattern.sh: scans ALL
# tracked .cs files (not just staged) so it works both as a CI step and locally.
#
# Exit codes: 0 = clean, 1 = anti-patterns found.
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

FILES=()
while IFS= read -r FILE; do
  [[ -n "$FILE" ]] && FILES+=("$FILE")
done < <((git ls-files -- '*.cs'; git ls-files --others --exclude-standard -- '*.cs') | sort -u | grep -v '/obj/\|/bin/' || true)

if [[ ${#FILES[@]} -eq 0 ]]; then
  echo "No .cs files to check."
  exit 0
fi

echo "Checking ${#FILES[@]} C# file(s) for anti-patterns..."
ERRORS=0

# Strips single-line comments (// ...) and whole-line block comments so that
# discussing an anti-pattern in a code comment does not trip the gate — only
# actual code is checked.
strip_comments() {
  sed -e 's://.*::' -e 's:^[[:space:]]*\*.*::' -e 's:^[[:space:]]*/\*.*::' "$1"
}

check_pattern() {
  local file="$1" pattern="$2" icon="$3" exclude="${4:-}"
  local hits
  if [[ -n "$exclude" ]]; then
    hits="$(strip_comments "$file" | grep -n "$pattern" | grep -v "$exclude" || true)"
  else
    hits="$(strip_comments "$file" | grep -n "$pattern" || true)"
  fi

  if [[ -n "$hits" ]]; then
    printf '%s\n' "$hits" | sed "s#^#${icon}  ${file}:#"
    ERRORS=$((ERRORS + 1))
  fi
}

for FILE in "${FILES[@]}"; do
  [[ -f "$FILE" ]] || continue

  check_pattern "$FILE" 'DateTime\.\(Now\|UtcNow\)' '⚠️'
  check_pattern "$FILE" 'new HttpClient()' '⚠️'
  check_pattern "$FILE" 'async void' '🔴' 'EventArgs'
  check_pattern "$FILE" '\.Result\b\|\.GetAwaiter()\.GetResult()' '🔴'
done

if [[ $ERRORS -gt 0 ]]; then
  echo ""
  echo "Found $ERRORS anti-pattern issue(s). Fix before merging (constitution v2.1.0 gate)."
  exit 1
fi

echo "No anti-patterns found."
exit 0
