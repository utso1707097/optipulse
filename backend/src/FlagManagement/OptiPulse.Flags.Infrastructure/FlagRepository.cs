using Microsoft.EntityFrameworkCore;
using OptiPulse.Flags.Application;
using OptiPulse.Flags.Domain;
using OptiPulse.SharedKernel;

namespace OptiPulse.Flags.Infrastructure;

public sealed class FlagRepository(FlagsDbContext dbContext) : IFlagRepository
{
    public Task<Flag?> GetByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        dbContext.Flags.FirstOrDefaultAsync(f => f.Key == key, cancellationToken);

    public async Task<IReadOnlyList<Flag>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Flags.OrderBy(f => f.Key).ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) =>
        dbContext.Flags.AnyAsync(f => f.Key == key, cancellationToken);

    public async Task AddAsync(Flag flag, CancellationToken cancellationToken = default) =>
        await dbContext.Flags.AddAsync(flag, cancellationToken);

    public async Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Version is mapped as EF's concurrency token, so a competing write makes the
            // UPDATE match zero rows and EF raises this. Translating it to a Conflict Result
            // is what turns a lost update into a 409 the caller can retry against fresh state
            // (FR-011) instead of an unhandled 500 — or worse, a silent overwrite.
            return Error.Conflict(
                "Flag.ConcurrencyConflict",
                "The flag was modified by another request. Re-read it and retry with the current version.");
        }
    }
}
