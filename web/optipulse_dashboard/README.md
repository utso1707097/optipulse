# OptiPulse Web Dashboard (React)

The always-online manager console for Product & Marketing: flag creation, experiment management,
and analytics review. Lightweight React + TypeScript (Vite) with
**Redux Toolkit** for client state and **Tailwind CSS** for styling (constitution v2.4.0). API access via a
TypeScript client generated from the backend OpenAPI spec.

- Design & constraints: [../../specs/001-optipulse-platform/plan.md](../../specs/001-optipulse-platform/plan.md) (Principle V)
- Contract generation: [../../specs/001-optipulse-platform/contracts/openapi-pipeline.md](../../specs/001-optipulse-platform/contracts/openapi-pipeline.md)
- Tasks: Phase 5 (US3) in [../../specs/001-optipulse-platform/tasks.md](../../specs/001-optipulse-platform/tasks.md)
- Best practices: `.kits/claude-code-best-practices` (tooling only — see plan.md; state guidance from
  that kit is intentionally NOT followed)

## Structure

```text
src/
├── api/             # schema.d.ts (GENERATED from the backend OpenAPI spec) + client.ts
├── store/           # Redux Toolkit slices: auth, flags, experiments, analytics
├── hooks/           # typed useAppSelector/useAppDispatch, useOnlineStatus
├── features/{auth,flags,experiments,analytics}/
└── components/      # AppShell, ConnectivityGuard, UI primitives
test/                # Vitest + Testing Library
```

## Commands

```bash
npm run dev      # start dev server
npm run build    # typecheck + production build
npm test         # run Vitest
npm run lint     # oxlint
npm run format   # prettier --write
```

## Running it locally

**Leave `VITE_API_URL` unset for local development.** With it unset the client makes same-origin
requests, and `vite.config.ts` proxies `/api` and `/health` to a backend:

```bash
# against a locally-running API (the default, http://localhost:5289)
npm run dev

# against the deployed API
OPTIPULSE_API_PROXY=https://optipulse-api.onrender.com npm run dev
```

**This is a proxy, not a CORS entry, and the difference matters.** The browser only ever sees
`http://localhost:5173`, so no cross-origin request is made and no preflight happens — CORS is
not involved at all. Adding `http://localhost:5173` to the deployed API's allowlist would instead
make production permanently trust an origin that any developer's machine can serve, including one
running something other than this dashboard. A local convenience should not widen what production
accepts.

If you set `VITE_API_URL` locally you opt out of the proxy and go cross-origin directly, which
then _does_ require the API to allow your origin. Don't, unless you have a reason.

## Environment (deployed builds)

`VITE_API_URL` must point at the deployed API — the dashboard and API are on different origins,
so a relative base URL cannot work in production. It is inlined by Vite at **build** time, so it
must be set before the build, not after. The API must also list this dashboard's origin in
`Cors__AllowedOrigins`, or the browser blocks every request before it reaches the server.

## Micro-copy

The AI micro-copy screen (T060) is deliberately absent: the AI Gateway is deferred past the MVP,
so there is no endpoint behind it. A screen wired to nothing is worse than no screen.
