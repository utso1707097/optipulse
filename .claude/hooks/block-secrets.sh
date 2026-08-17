#!/usr/bin/env bash
# PreToolUse(Write|Edit): block writes that introduce obvious secrets.
# Exit 2 = block (message on stderr). Adopted from the claude-code-best-practices
# kit's block-secrets hook, then tightened (see NOTE) to remove false positives.
set -uo pipefail

payload="$(cat)"
content="$(printf '%s' "$payload" | python3 -c 'import json,sys
d=json.load(sys.stdin).get("tool_input",{})
print(d.get("content", d.get("new_string","")))' 2>/dev/null)"
[ -z "$content" ] && exit 0

# Constitution Principle VI: signing secrets must never leave the backend / land in clients.
#
# NOTE on precision: the generic name/value rule below requires the value to be a
# QUOTED STRING LITERAL. The original rule allowed an unquoted value, so any
# ordinary assignment of an identifier-like expression to a variable whose name
# ended in a sensitive word (e.g. a local holding a freshly minted JWT assigned
# from a factory method) matched and blocked legitimate authentication code.
# Requiring the quotes keeps the rule sharp — genuine hardcoded credentials are
# string literals — while eliminating that entire class of false positive. The
# provider-specific formats (AWS access key IDs, PEM private-key blocks, Slack
# and GitHub tokens) are distinctive enough to match without a quote anchor.
KEY_NAMES='secret|Secret|SECRET|password|Password|PASSWORD|passwd|api[_-]?key|API[_-]?KEY|apiKey|ApiKey'
QUOTED_VALUE='["'"'"'][A-Za-z0-9/+_-]{16,}["'"'"']'

if printf '%s' "$content" | grep -Eq \
  -e 'AKIA[0-9A-Z]{16}' \
  -e '-----BEGIN (RSA |EC |OPENSSH |DSA |PGP )?PRIVATE KEY-----' \
  -e '(xox[baprs]-[0-9A-Za-z-]{10,})' \
  -e 'gh[pousr]_[0-9A-Za-z]{36,}' \
  -e "(${KEY_NAMES})[[:space:]]*[:=][[:space:]]*${QUOTED_VALUE}"; then
  echo "BLOCKED by block-secrets hook: the content appears to contain a hardcoded secret (key/token/password)." >&2
  echo "Move secrets to configuration/environment/secret store; do not commit them (Constitution Principle VI)." >&2
  exit 2
fi
exit 0
