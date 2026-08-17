# OptiPulse Backend — Claude Code Guidance

.NET 10 Web API. Read [`.specify/memory/constitution.md`](../.specify/memory/constitution.md) first —
this file only adds backend-specific operating detail on top of it.

## Architecture — pinned to Clean Architecture

This project uses **Clean Architecture** across four DDD bounded contexts (Evaluation Engine, Flag
Management, AI Gateway, Audit & Telemetry) plus the `OptiPulse.IdentityAccess` supporting subdomain.
Each context is `Domain` → `Application` → `Infrastructure`, inward-only dependencies, composed by
`OptiPulse.Api`.

Adopted from `.kits/dotnet-claude-kit` (git-ignored, local reference only — do not depend on it being
present in CI): the `clean-architecture` skill matches this layout exactly. **Do not** let the kit's
architecture-advisor or scaffolding offer Vertical Slice / DDD-flat / Modular-Monolith — this project
does not use them.

## Non-negotiable pins (differ from the kit's defaults)

- **Auth**: custom JWT/RBAC in `OptiPulse.IdentityAccess`, issued/validated entirely server-side.
  Do **not** reach for ASP.NET Identity or an external OIDC provider — the kit's `authentication`
  skill defaults there; override it.
- **Cache invalidation**: Redis Pub/Sub + fail-safe-to-last-known-good, **not** the kit's HybridCache
  tag-invalidation pattern.
- **Evaluation hot path** (`EvaluationEngine.*`): **no Mediator, no HybridCache, no LINQ, no boxing**
  on this path. Zero-allocation only — `Span<T>`, `stackalloc`, struct types, MurmurHash3. Mediator/
  caching indirection is fine for management/CRUD endpoints, never here.
- **Central package management**: all versions live in `Directory.Packages.props`; never add an
  inline `Version=` to a `PackageReference`.
- **Warnings are errors**: `Directory.Build.props` sets `TreatWarningsAsErrors`, `EnableAotAnalyzer`,
  `EnableTrimAnalyzer`. A change that introduces a new warning fails the build by design.
- **Anti-pattern gate**: `scripts/check-antipatterns.sh` (CI-enforced) blocks `DateTime.Now`/
  `DateTime.UtcNow` (use `TimeProvider`), `new HttpClient()` (use `IHttpClientFactory`), `async void`,
  and `.Result`/`.GetAwaiter().GetResult()` sync-over-async.

## Tooling available

- **Roslyn navigator MCP** (`cwm-roslyn-navigator`, registered in `../.mcp.json`) — prefer it over
  raw file greps for symbol lookup, call graphs, dead-code/circular-dependency detection once the
  solution exists.
- `.claude/hooks/format-on-write.sh` runs `dotnet format` automatically on every `.cs` write.

## Structure

See [`../specs/001-optipulse-platform/plan.md`](../specs/001-optipulse-platform/plan.md) for the full
project tree and [`tasks.md`](../specs/001-optipulse-platform/tasks.md) for the task-by-task build order.
