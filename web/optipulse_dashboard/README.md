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

## Environment

`VITE_API_URL` must point at the deployed API — the dashboard and API are on different origins,
so a relative base URL only works behind a local dev proxy. Copy `.env.example` to `.env.local`
for local work. The API must also list this dashboard's origin in `Cors__AllowedOrigins`, or the
browser blocks every request before it reaches the server.

## Micro-copy

The AI micro-copy screen (T060) is deliberately absent: the AI Gateway is deferred past the MVP,
so there is no endpoint behind it. A screen wired to nothing is worse than no screen.
