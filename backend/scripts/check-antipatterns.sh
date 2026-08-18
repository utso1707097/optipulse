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
  # `.Result` here means sync-over-async (blocking on a Task). Exclude type references such as
  # `Task<Result<Flag>>` or `SharedKernel.Result`, which are followed by '<' or '>' — blocking on
  # a task's .Result never is. Without this the gate flagged the Result pattern the constitution
  # itself mandates for expected failures.
  check_pattern "$FILE" '\.Result\b\|\.GetAwaiter()\.GetResult()' '🔴' '\.Result[<>]'
done

# Constitution v2.2.0 Principle IV: "a registered resilience pipeline MUST have at least one
# consumer. A pipeline that is registered and never used is a governance violation, not
# compliance." This existed for real — two pipelines were registered and wired to nothing, so
# the rule read as satisfied while the dependencies were unprotected. Gate it rather than
# trusting review to notice.
# `|| true` on both greps: with `set -euo pipefail`, a no-match grep (exit 1) would abort the
# whole gate silently, which would be a gate that stops checking rather than one that passes.
PIPELINE_NAMES=$( (grep -rhoE 'AddResiliencePipeline\([A-Za-z]+' --include='*.cs' src/ 2>/dev/null || true) \
                  | sed -E 's/AddResiliencePipeline\(//' | sort -u )
for NAME in $PIPELINE_NAMES; do
  # Consumers usually qualify the constant (ResilienceExtensions.RedisPipeline), so match a
  # trailing identifier path rather than the bare name.
  CONSUMERS=$( (grep -rlE "GetPipeline\([A-Za-z.]*$NAME" --include='*.cs' src/ 2>/dev/null || true) | wc -l | tr -d ' ')
  if [[ "$CONSUMERS" -eq 0 ]]; then
    echo "🔴  Resilience pipeline '$NAME' is registered but never consumed (no GetPipeline($NAME))."
    echo "    Wire it to a call site or remove it — constitution v2.2.0 Principle IV."
    ERRORS=$((ERRORS + 1))
  fi
done

if [[ $ERRORS -gt 0 ]]; then
  echo ""
  echo "Found $ERRORS anti-pattern issue(s). Fix before merging (constitution v2.1.0 gate)."
  exit 1
fi

echo "No anti-patterns found."
exit 0
