using Microsoft.EntityFrameworkCore;
using OptiPulse.Audit.Application;

namespace OptiPulse.Audit.Infrastructure;

public sealed class ExposureAggregator(AuditDbContext dbContext) : IExposureAggregator
{
    public Task<long> GetExposureCountAsync(string flagKey, CancellationToken cancellationToken = default) =>
        dbContext.ExposureEvents.LongCountAsync(e => e.FlagKey == flagKey, cancellationToken);
}
