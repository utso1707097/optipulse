#!/usr/bin/env bash
#
# Post-deployment smoke test.
#
# Checks the things that unit and integration tests CANNOT reach, because they only exist once
# the two halves are deployed to different origins: whether the API is awake, whether the
# dashboard was actually built with the right API URL baked in, and whether CORS lets the
# browser through. Every one of those has failed silently on this project at least once.
#
# It uses NO credentials. It deliberately verifies that protected endpoints REFUSE anonymous
# callers rather than logging in to prove they accept authorised ones — a smoke test should not
# need a password, and one that holds a password becomes a thing you have to protect.
#
# Usage:
#   scripts/smoke-deployment.sh https://optipulse-api.onrender.com https://optipulse.vercel.app

set -uo pipefail

API_URL="${1:-${RENDER_API_BASE_URL:-}}"
WEB_URL="${2:-${DASHBOARD_URL:-}}"

if [ -z "$API_URL" ] || [ -z "$WEB_URL" ]; then
  echo "Usage: $0 <api-url> <dashboard-url>" >&2
  echo "   e.g. $0 https://optipulse-api.onrender.com https://optipulse.vercel.app" >&2
  exit 2
fi

API_URL="${API_URL%/}"
WEB_URL="${WEB_URL%/}"

PASS=0
FAIL=0
NOTE=""

ok()   { printf '  \033[32mPASS\033[0m  %s\n' "$1"; PASS=$((PASS + 1)); }
bad()  { printf '  \033[31mFAIL\033[0m  %s\n' "$1"; printf '        %s\n' "$2"; FAIL=$((FAIL + 1)); }
info() { printf '        %s\n' "$1"; }

echo
echo "API:       $API_URL"
echo "Dashboard: $WEB_URL"
echo

# ---------------------------------------------------------------------------------------------
echo "1. API reachability"
# ---------------------------------------------------------------------------------------------

# Render's free tier spins containers down after inactivity; the first request pays a cold start
# of up to ~60s. A short timeout here would report a healthy service as dead.
echo "   (cold start on a free instance can take up to a minute — waiting)"
LIVE_BODY=$(curl -sS --max-time 90 -w '\n%{http_code}' "$API_URL/health/live" 2>&1)
LIVE_CODE=$(printf '%s' "$LIVE_BODY" | tail -n1)

if [ "$LIVE_CODE" = "200" ]; then
  ok "/health/live responded 200"
else
  bad "/health/live did not respond 200 (got '${LIVE_CODE}')" \
      "The API host is down, still deploying, or the URL is wrong."
fi

READY=$(curl -sS --max-time 90 -w '\n%{http_code}' "$API_URL/health/ready" 2>&1)
READY_CODE=$(printf '%s' "$READY" | tail -n1)
READY_BODY=$(printf '%s' "$READY" | sed '$d')

if [ "$READY_CODE" = "200" ]; then
  ok "/health/ready responded 200 (database and Redis reachable)"
else
  # Truncated: when the URL points somewhere unexpected the "body" is a whole HTML page, and
  # dumping it buries the one line that says what failed.
  bad "/health/ready responded ${READY_CODE}" \
      "The host is running but a dependency is not. Body: $(printf '%s' "$READY_BODY" | head -c 200)"
fi

# ---------------------------------------------------------------------------------------------
echo
echo "2. API is actually protected"
# ---------------------------------------------------------------------------------------------

FLAGS_CODE=$(curl -sS --max-time 30 -o /dev/null -w '%{http_code}' "$API_URL/api/v1/flags")
if [ "$FLAGS_CODE" = "401" ]; then
  ok "GET /api/v1/flags refuses an anonymous caller (401)"
elif [ "$FLAGS_CODE" = "200" ]; then
  bad "GET /api/v1/flags returned 200 WITHOUT a token" \
      "The management API is publicly readable. This is a security problem, not a config nit."
else
  bad "GET /api/v1/flags returned ${FLAGS_CODE}, expected 401" \
      "Neither protected-as-expected nor obviously open — check the auth configuration."
fi

EVAL_CODE=$(curl -sS --max-time 30 -o /dev/null -w '%{http_code}' \
  -X POST "$API_URL/api/v1/evaluate" \
  -H 'Content-Type: application/json' \
  -d '{"flagKey":"smoke.probe","contextKey":null,"attributes":null}')
if [ "$EVAL_CODE" = "401" ]; then
  ok "POST /api/v1/evaluate requires a service-account key (401)"
else
  bad "POST /api/v1/evaluate returned ${EVAL_CODE}, expected 401" \
      "The SDK evaluation surface should reject callers with no X-OptiPulse-Key."
fi

# ---------------------------------------------------------------------------------------------
echo
echo "3. CORS — can the browser reach the API from the dashboard?"
# ---------------------------------------------------------------------------------------------
#
# This is the check that matters most after a fresh deploy, and the one no local test can make.
# If Cors__AllowedOrigins does not contain the dashboard's exact origin, the browser blocks
# every request BEFORE it reaches the server — so the API looks perfectly healthy from curl
# while the dashboard shows nothing but errors.

PREFLIGHT=$(curl -sS --max-time 30 -D - -o /dev/null \
  -X OPTIONS "$API_URL/api/v1/flags" \
  -H "Origin: $WEB_URL" \
  -H 'Access-Control-Request-Method: GET' \
  -H 'Access-Control-Request-Headers: authorization,content-type' 2>&1)

ALLOW_ORIGIN=$(printf '%s' "$PREFLIGHT" \
  | tr -d '\r' \
  | grep -i '^access-control-allow-origin:' \
  | head -n1 | cut -d' ' -f2-)

if [ -z "$ALLOW_ORIGIN" ]; then
  bad "Preflight returned no Access-Control-Allow-Origin header" \
      "The browser will block every call. Set Cors__AllowedOrigins__0=${WEB_URL} on Render and redeploy."
elif [ "$ALLOW_ORIGIN" = "$WEB_URL" ]; then
  ok "Preflight allows ${WEB_URL}"
elif [ "$ALLOW_ORIGIN" = "*" ]; then
  bad "Preflight returned a wildcard origin" \
      "Prohibited by constitution v2.3.0: this API carries bearer credentials and an Admin kill-switch."
else
  bad "Preflight allows '${ALLOW_ORIGIN}', not '${WEB_URL}'" \
      "Origins must match EXACTLY — scheme, host and port, no trailing slash."
fi

ALLOW_CREDS=$(printf '%s' "$PREFLIGHT" | tr -d '\r' \
  | grep -i '^access-control-allow-headers:' | head -n1)
if printf '%s' "$ALLOW_CREDS" | grep -qi 'authorization'; then
  ok "Preflight allows the Authorization header"
else
  NOTE="${NOTE}\n  - Preflight did not echo the Authorization header; authenticated calls may still be blocked."
fi

# ---------------------------------------------------------------------------------------------
echo
echo "4. Dashboard"
# ---------------------------------------------------------------------------------------------

HOME_CODE=$(curl -sS --max-time 30 -o /dev/null -w '%{http_code}' "$WEB_URL/")
if [ "$HOME_CODE" = "200" ]; then
  ok "Dashboard root responded 200"
else
  bad "Dashboard root responded ${HOME_CODE}" "Is the Vercel deployment live and public?"
fi

# A direct hit on a client-side route. Without an SPA rewrite this 404s, so the app works when
# you click through it and breaks the moment anyone refreshes or shares a link.
ROUTE_CODE=$(curl -sS --max-time 30 -o /dev/null -w '%{http_code}' "$WEB_URL/flags")
if [ "$ROUTE_CODE" = "200" ]; then
  ok "Deep link /flags served (SPA rewrite is working)"
else
  bad "Deep link /flags returned ${ROUTE_CODE}" \
      "Missing SPA fallback — check that web/optipulse_dashboard/vercel.json was deployed."
fi

# The API URL is inlined into the bundle at BUILD time. If VITE_API_URL was missing when Vercel
# built, the bundle silently points at its own origin and every call 404s — with no error
# anywhere in the deploy log. This is the single easiest way to ship a broken dashboard.
BUNDLE_URL=$(curl -sS --max-time 30 "$WEB_URL/" \
  | grep -o '/assets/index-[A-Za-z0-9_-]*\.js' | head -n1)

if [ -z "$BUNDLE_URL" ]; then
  NOTE="${NOTE}\n  - Could not locate the JS bundle to verify the baked-in API URL."
else
  API_HOST="${API_URL#https://}"
  API_HOST="${API_HOST#http://}"
  if curl -sS --max-time 60 "${WEB_URL}${BUNDLE_URL}" | grep -q "$API_HOST"; then
    ok "Built bundle contains the API host (VITE_API_URL was set at build time)"
  else
    bad "Built bundle does NOT reference ${API_HOST}" \
        "VITE_API_URL was missing or wrong when Vercel built. Set it and REBUILD — a static bundle has no runtime env, so restarting will not help."
  fi
fi

# ---------------------------------------------------------------------------------------------
echo
if [ -n "$NOTE" ]; then
  printf 'Notes:%b\n\n' "$NOTE"
fi

printf '%s passed, %s failed\n\n' "$PASS" "$FAIL"
[ "$FAIL" -eq 0 ] || exit 1
echo "Deployment looks healthy."
