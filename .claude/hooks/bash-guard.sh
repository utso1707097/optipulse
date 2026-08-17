#!/usr/bin/env bash
# PreToolUse(Bash): block destructive commands. Exit 2 = block (message on stderr).
# Adopted from the .NET kit's pre-bash-guard hook.
set -uo pipefail

payload="$(cat)"
cmd="$(printf '%s' "$payload" | python3 -c 'import json,sys;print(json.load(sys.stdin).get("tool_input",{}).get("command",""))' 2>/dev/null)"
[ -z "$cmd" ] && exit 0

if printf '%s' "$cmd" | grep -Eq \
  -e 'git[[:space:]]+push([[:space:]][^|;&]*)?[[:space:]](--force|-f)([[:space:]]|$)' \
  -e 'git[[:space:]]+push[[:space:]][^|;&]*[[:space:]]\+[A-Za-z]' \
  -e 'git[[:space:]]+reset[[:space:]]+--hard' \
  -e 'git[[:space:]]+clean[[:space:]]+-[a-z]*f[a-z]*d' \
  -e 'rm[[:space:]]+-[a-z]*r[a-z]*f[a-z]*[[:space:]]+/([[:space:]]|$)' \
  -e 'rm[[:space:]]+-[a-z]*f[a-z]*r[a-z]*[[:space:]]+/([[:space:]]|$)'; then
  echo "BLOCKED by bash-guard hook: destructive command (force-push / reset --hard / clean -fd / rm -rf /)." >&2
  echo "If this is truly intended, run it yourself outside the agent (Constitution git-workflow guardrail)." >&2
  exit 2
fi
exit 0
