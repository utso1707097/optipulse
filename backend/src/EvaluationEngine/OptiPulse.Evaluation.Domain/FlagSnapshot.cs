using System.Collections.Frozen;

namespace OptiPulse.Evaluation.Domain;

/// <summary>
/// Immutable, in-memory snapshot of all active flags (research R2). Lookups are
/// O(1) via FrozenDictionary with zero locking on the read path — readers take a
/// single reference and never block writers or each other. Never persisted; a
/// fresh snapshot is built from FlagManagement's authoritative store at startup
/// and refreshed via Redis Pub/Sub deltas (contracts/invalidation-channel.md).
/// </summary>
public sealed class FlagSnapshot
{
    public long Version { get; }
    public DateTimeOffset BuiltAt { get; }
    private readonly FrozenDictionary<string, CompiledFlag> _flags;

    public FlagSnapshot(long version, DateTimeOffset builtAt, IEnumerable<CompiledFlag> flags)
    {
        Version = version;
        BuiltAt = builtAt;
        _flags = flags.ToFrozenDictionary(f => f.FlagKey);
    }

    public static FlagSnapshot Empty { get; } = new(0, DateTimeOffset.MinValue, []);

    public bool TryGetFlag(string flagKey, out CompiledFlag? flag) => _flags.TryGetValue(flagKey, out flag);

    /// <summary>
    /// Read surface for operational reporting (T071). Not used by the evaluation path — that
    /// goes through <see cref="TryGetFlag"/>, which stays a single FrozenDictionary lookup with
    /// no enumeration and no allocation.
    /// </summary>
    public int Count => _flags.Count;

    public IReadOnlyCollection<CompiledFlag> Flags => _flags.Values;

    /// <summary>Returns a new snapshot with the given flag replaced/added and the
    /// version advanced — used to apply a single-flag delta (invalidation
    /// subscriber) without rebuilding the whole snapshot from scratch.</summary>
    public FlagSnapshot WithUpdatedFlag(CompiledFlag updated, long newVersion, DateTimeOffset now)
    {
        var merged = _flags.Values.Where(f => f.FlagKey != updated.FlagKey).Append(updated);
        return new FlagSnapshot(newVersion, now, merged);
    }
}
