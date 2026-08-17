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

# Files permitted to name a clock directly. Keep this list empty-by-default and justify any
# entry: every exemption is a place where time cannot be controlled in a test.
EXEMPT_TIME_FILES=""

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

  # Constitution v2.2.0: DateTimeOffset is banned alongside DateTime in PRODUCTION code. The
  # earlier DateTime-only pattern let `DateTimeOffset.UtcNow` through, so audit timestamps and
  # snapshot times were unmockable while this gate still reported clean.
  #
  # Scoped to src/ deliberately: the rule exists so production time is injectable and therefore
  # controllable in a test. A test constructing its own fixed timestamp is the goal, not a
  # violation of it — banning it there would push tests toward indirection for no benefit.
  if [[ "$FILE" == src/* ]] && [[ " $EXEMPT_TIME_FILES " != *" $FILE "* ]]; then
    check_pattern "$FILE" 'DateTime\(Offset\)\?\.\(Now\|UtcNow\)' '⚠️'
  fi
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
