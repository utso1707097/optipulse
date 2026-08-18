using Microsoft.EntityFrameworkCore;
using OptiPulse.Audit.Application;

namespace OptiPulse.Audit.Infrastructure;

public sealed class ExposureAggregator(AuditDbContext dbContext) : IExposureAggregator
{
    public Task<long> GetExposureCountAsync(string flagKey, CancellationToken cancellationToken = default) =>
        dbContext.ExposureEvents.LongCountAsync(e => e.FlagKey == flagKey, cancellationToken);

    public async Task<IReadOnlyList<VariantExposureCount>> GetVariantExposureCountsAsync(
        string flagKey, CancellationToken cancellationToken = default)
    {
        // Grouped in the database rather than by materialising rows and counting in memory:
        // exposures are the highest-volume table in the system, and pulling them into the API
        // process to count them would not survive real traffic.
        var grouped = await dbContext.ExposureEvents
            .Where(e => e.FlagKey == flagKey)
            .GroupBy(e => e.VariantKey)
            .Select(g => new { VariantKey = g.Key, Exposures = g.LongCount() })
            .ToListAsync(cancellationToken);

        return grouped
            .Select(g => new VariantExposureCount(g.VariantKey, g.Exposures))
            .OrderBy(g => g.VariantKey)
            .ToList();
    }
}