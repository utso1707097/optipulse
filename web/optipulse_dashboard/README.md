# OptiPulse Web Dashboard (React)

The always-online manager console for Product & Marketing: flag creation, experiment management,
AI micro-copy generation/approval, and analytics review. Lightweight React + TypeScript (Vite) with
**standard custom hooks** and a single `AuthContext` — **no Redux/MobX/Zustand**. API access via a
TypeScript client generated from the backend OpenAPI spec.

- Design & constraints: [../../specs/001-optipulse-platform/plan.md](../../specs/001-optipulse-platform/plan.md) (Principle V)
- Contract generation: [../../specs/001-optipulse-platform/contracts/openapi-pipeline.md](../../specs/001-optipulse-platform/contracts/openapi-pipeline.md)
- Tasks: Phase 5 (US3) in [../../specs/001-optipulse-platform/tasks.md](../../specs/001-optipulse-platform/tasks.md)
- Best practices: `.kits/claude-code-best-practices` (tooling only — see plan.md; state guidance from
  that kit is intentionally NOT followed)

## Structure

```text
src/
├── api/            # Generated TypeScript client (from backend OpenAPI spec)
├── context/         # AuthContext — the only cross-cutting global state
├── hooks/            # useFlags, useExperiments, useMicroCopy, useAnalytics, useAuth
├── features/{flags,experiments,microcopy,analytics}/
└── components/
test/                 # Vitest + Testing Library
```

## Commands

```bash
npm run dev      # start dev server
npm run build    # typecheck + production build
npm test         # run Vitest
npm run lint     # oxlint
npm run format   # prettier --write
```
