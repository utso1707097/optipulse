using Microsoft.EntityFrameworkCore;
using OptiPulse.Flags.Application;
using OptiPulse.Flags.Domain;
using OptiPulse.SharedKernel;

namespace OptiPulse.Flags.Infrastructure;

public sealed class ExperimentRepository(FlagsDbContext dbContext) : IExperimentRepository
{
    public Task<Experiment?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Experiments.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Experiment>> ListAsync(
        string? flagKey = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Experiments.AsQueryable();
        if (!string.IsNullOrWhiteSpace(flagKey))
            query = query.Where(e => e.FlagKey == flagKey);

        return await query.OrderBy(e => e.Name).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Experiment experiment, CancellationToken cancellationToken = default) =>
        await dbContext.Experiments.AddAsync(experiment, cancellationToken);

    public async Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error.Conflict(
                "Experiment.ConcurrencyConflict",
                "The experiment was modified by another request. Re-read it and retry.");
        }
    }
}
