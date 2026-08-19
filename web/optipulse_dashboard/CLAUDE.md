# OptiPulse Web Dashboard — Claude Code Guidance

React + TypeScript (Vite), the always-online Manager console. Read
[`.specify/memory/constitution.md`](../../.specify/memory/constitution.md) first (Principle V) —
this file only adds frontend-specific operating detail on top of it.

## State management — pinned, no exceptions

- **Redux Toolkit** (constitution v2.4.0) is the state model. Slices live in `src/store/`
  (`authSlice`, `flagsSlice`, `experimentsSlice`, `analyticsSlice`); components read and write
  through the typed hooks in `src/hooks/` (`useAppSelector`, `useAppDispatch`), never the untyped
  react-redux originals.
- Use RTK slices and `createAsyncThunk`, not hand-written reducers. Do **not** add MobX, Zustand,
  or React Query/TanStack Query alongside it — one state library is enough.
- **Server state stays authoritative in the backend.** A slice may hold fetched data for
  rendering; it must not become a second source of truth. Every mutation thunk therefore re-reads
  the list from the server rather than patching the local copy, and a failed read CLEARS the
  slice instead of leaving stale rows on screen.
- **No client-side domain layer.** Targeting, rollout bucketing, kill-switch precedence and
  authorization are backend rules. `auth.role` exists to decide which controls are _rendered_;
  it never decides what is _permitted_.
- **Styling: Tailwind CSS** (v2.4.0). Utility classes in components are expected. Design tokens
  live in the `@theme` block in `src/index.css` — there is no `tailwind.config.js` (Tailwind v4).
- Superseded guidance, kept for context: `.kits/claude-code-best-practices` (git-ignored, local
  reference only) ships React example docs recommending Zustand and React Query. That guidance is
  explicitly **not followed** here. The earlier `src/context/AuthContext.tsx` pin was replaced by
  `src/store/authSlice.ts` when v2.4.0 permitted Redux.

## API contract

- `src/api/schema.d.ts` is **generated** from the backend's native OpenAPI spec by
  `../../contracts-gen/generate.sh` — never hand-edit it, and never let Prettier reformat it
  (see `.prettierignore`; a reformat is an automatic drift-gate failure).
  See [`../../specs/001-optipulse-platform/contracts/openapi-pipeline.md`](../../specs/001-optipulse-platform/contracts/openapi-pipeline.md).
- `src/api/client.ts` is the hand-written typed client on top of it. It derives every request and
  response type from the generated schema, so a contract change breaks the build rather than
  causing a runtime surprise. It also owns bearer-token injection, the single-attempt refresh on
  401, `If-Match` version headers, and ProblemDetails parsing.

## Always-online

This dashboard assumes network availability — no offline persistence, no service worker caching
of mutable state (spec FR-029). `src/components/ConnectivityGuard.tsx` enforces it: when the
browser reports offline it **replaces** the work area rather than dimming it, so there is no
stale editable state on screen at all.

## Tooling available

- `.claude/hooks/format-on-write.sh` runs `prettier` automatically on every write.
- `.claude/hooks/block-secrets.sh` blocks committing hardcoded secrets/tokens.

## Commands

```bash
npm run dev / build / test / lint / format
```
