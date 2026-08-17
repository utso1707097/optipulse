#!/usr/bin/env bash
# PreToolUse(Write|Edit): block writes that introduce obvious secrets.
# Exit 2 = block (message on stderr). Adopted from the claude-code-best-practices
# kit's block-secrets hook. Conservative patterns to avoid false positives.
set -uo pipefail

payload="$(cat)"
content="$(printf '%s' "$payload" | python3 -c 'import json,sys
d=json.load(sys.stdin).get("tool_input",{})
print(d.get("content", d.get("new_string","")))' 2>/dev/null)"
[ -z "$content" ] && exit 0

# Constitution Principle VI: signing secrets must never leave the backend / land in clients.
if printf '%s' "$content" | grep -Eq \
  -e 'AKIA[0-9A-Z]{16}' \
  -e '-----BEGIN (RSA |EC |OPENSSH |DSA |PGP )?PRIVATE KEY-----' \
  -e '(xox[baprs]-[0-9A-Za-z-]{10,})' \
  -e 'gh[pousr]_[0-9A-Za-z]{36,}' \
  -e '(?i)(secret|password|passwd|api[_-]?key|token)[[:space:]]*[:=][[:space:]]*["'"'"']?[A-Za-z0-9/+_-]{16,}'; then
  echo "BLOCKED by block-secrets hook: the content appears to contain a hardcoded secret (key/token/password)." >&2
  echo "Move secrets to configuration/environment/secret store; do not commit them (Constitution Principle VI)." >&2
  exit 2
fi
exit 0
