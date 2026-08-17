using OptiPulse.Evaluation.Application;
using OptiPulse.Evaluation.Domain;

namespace OptiPulse.Evaluation.Infrastructure;

/// <summary>
/// Lock-free, atomically-swapped snapshot reference (research R2). Reads take
/// the current reference with no locking and never block writers or each other
/// — reference assignment is atomic at the CLR level, and `volatile` guarantees
/// cross-thread visibility of the latest published snapshot. If a refresh never
/// arrives (dependency outage), the last successfully applied snapshot simply
/// stays in place — the fail-safe-to-last-known-good behavior (FR-005) falls
/// out of this design rather than needing special-case error handling.
/// </summary>
/// <remarks>
/// <see cref="TimeProvider"/> is injected per constitution v2.2.0 so the snapshot's
/// <c>BuiltAt</c> is controllable in tests. It is only read on <see cref="ApplyDelta"/>
/// (the invalidation path), never on the evaluation read path, so this does not put an
/// indirection on the hot path that Principle II protects.
/// </remarks>
public sealed class SnapshotStore(TimeProvider timeProvider) : ISnapshotStore, ISnapshotWriter
{
    private volatile FlagSnapshot _current = FlagSnapshot.Empty;

    public FlagSnapshot Current => _current;

    public void LoadInitial(FlagSnapshot snapshot) => _current = snapshot;

    public void ApplyDelta(CompiledFlag updated, long newVersion)
    {
        // Idempotent under at-most-once delivery: ignore stale/duplicate deltas
        // — but staleness is checked against THIS flag's own last-applied
        // version, not the global snapshot version. Flags are versioned
        // independently (each starts at 1), so comparing an incoming delta for
        // flag B against the snapshot-wide max (which may already be high
        // because of unrelated flag A) would incorrectly reject flag B's very
        // first legitimate update. The snapshot-wide Version field still tracks
        // the highest version seen across all flags (contracts/invalidation-
        // channel.md / data-model.md), for /snapshot/version reporting only.
        if (_current.TryGetFlag(updated.FlagKey, out var existing) &&
            existing is not null &&
            newVersion <= existing.Version)
        {
            return;
        }

        var snapshotVersion = Math.Max(_current.Version, newVersion);
        _current = _current.WithUpdatedFlag(updated, snapshotVersion, timeProvider.GetUtcNow());
    }
}
