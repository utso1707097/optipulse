# OptiPulse Web Dashboard — Claude Code Guidance

React + TypeScript (Vite), the always-online Manager console. Read
[`.specify/memory/constitution.md`](../../.specify/memory/constitution.md) first (Principle V) —
this file only adds frontend-specific operating detail on top of it.

## State management — pinned, no exceptions

- **Custom hooks + a single `AuthContext`** is the entire state model. `src/context/AuthContext.tsx`
  is the only cross-cutting global state in this app.
- **Do NOT add Redux, MobX, Zustand, or React Query/TanStack Query.** `.kits/claude-code-best-practices`
  (git-ignored, local reference only) ships React example docs that recommend Zustand and React
  Query — that guidance is explicitly **not followed** here; it conflicts with the constitution.
- Server data fetching goes through the hooks in `src/hooks/` (`useFlags`, `useExperiments`,
  `useMicroCopy`, `useAnalytics`, `useAuth`), which wrap the generated typed API client.

## API contract

- `src/api/` is **generated** from the backend's native OpenAPI spec by
  `../../contracts-gen/generate.sh` — never hand-edit files there.
  See [`../../specs/001-optipulse-platform/contracts/openapi-pipeline.md`](../../specs/001-optipulse-platform/contracts/openapi-pipeline.md).

## Always-online

This dashboard assumes network availability — no offline persistence, no service worker caching of
mutable state. When the network is unavailable, show a clear "requires connectivity" state rather
than stale editable data (spec FR-029).

## Tooling available

- `.claude/hooks/format-on-write.sh` runs `prettier` automatically on every write.
- `.claude/hooks/block-secrets.sh` blocks committing hardcoded secrets/tokens.
- Adopted from `.kits/claude-code-best-practices`: the `component-new` / `test-component` skill
  patterns are usable as references, adapted to this app's conventions (custom hooks, no Redux).

## Commands

```bash
npm run dev / build / test / lint / format
```
