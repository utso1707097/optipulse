using OptiPulse.Audit.Application;
using OptiPulse.Audit.Domain;
using OptiPulse.SharedKernel;

namespace OptiPulse.Audit.Infrastructure;

public sealed class AuditLog(AuditDbContext dbContext, TimeProvider timeProvider) : IAuditLog
{
    public async Task AppendAsync(
        ActorReference actor,
        AuditChangeType changeType,
        Guid targetId,
        string? beforeStateJson = null,
        string? afterStateJson = null,
        CancellationToken cancellationToken = default)
    {
        var entry = AuditEntry.Create(
            actor, changeType, targetId, timeProvider.GetUtcNow(), beforeStateJson, afterStateJson);
        await dbContext.AuditEntries.AddAsync(entry, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
