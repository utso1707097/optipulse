using Microsoft.EntityFrameworkCore;
using OptiPulse.Flags.Application;
using OptiPulse.Flags.Domain;

namespace OptiPulse.Flags.Infrastructure;

public sealed class FlagConfigurationReader(FlagsDbContext dbContext) : IFlagConfigurationReader
{
    public async Task<IReadOnlyList<Flag>> GetActiveFlagsAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Flags
            .Where(f => f.Status == FlagStatus.Active)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
}
