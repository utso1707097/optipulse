using OptiPulse.Flags.Domain;
using OptiPulse.SharedKernel;

namespace OptiPulse.Flags.Application;

/// <summary>
/// Persistence port for the Flag aggregate, owned by Application (Principle I: the interface
/// belongs to the inner layer, the EF implementation to Infrastructure).
/// </summary>
public interface IFlagRepository
{
    Task<Flag?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Flag>> ListAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task AddAsync(Flag flag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists pending changes. Returns a Conflict error rather than throwing when another
    /// writer changed the row first (FR-011) — a concurrent edit is an expected outcome of the
    /// management API, not an exceptional one, so it travels as a Result like other refusals.
    /// </summary>
    Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default);
}
