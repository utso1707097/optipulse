using Microsoft.Extensions.DependencyInjection;
using OptiPulse.Audit.Application;
using OptiPulse.Audit.Domain;
using OptiPulse.Resilience;
using OptiPulse.SharedKernel;
using Polly;
using Polly.Registry;

namespace OptiPulse.Audit.Infrastructure;

/// <summary>
/// Audit writes go through the named "postgres" resilience pipeline (Principle IV: outbound
/// persistence calls carry timeout + jittered retry + circuit breaker). This is a management
/// path, not the evaluation hot path, so the indirection is allowed — and audit entries are
/// exactly the writes worth retrying, since losing one to a transient blip loses the record of
/// a privileged action (FR-A07).
/// </summary>
public sealed class AuditLog : IAuditLog
{
    private readonly AuditDbContext _dbContext;
    private readonly TimeProvider _timeProvider;
    private readonly ResiliencePipeline _pipeline;

    public AuditLog(
        AuditDbContext dbContext,
        TimeProvider timeProvider,
        ResiliencePipelineProvider<string> pipelineProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
        _pipeline = pipelineProvider.GetPipeline(ResilienceExtensions.PostgresPipeline);
    }

    public async Task AppendAsync(
        ActorReference actor,
        AuditChangeType changeType,
        Guid targetId,
        string? beforeStateJson = null,
        string? afterStateJson = null,
        CancellationToken cancellationToken = default)
    {
        var entry = AuditEntry.Create(
            actor, changeType, targetId, _timeProvider.GetUtcNow(), beforeStateJson, afterStateJson);

        await _dbContext.AuditEntries.AddAsync(entry, cancellationToken);

        await _pipeline.ExecuteAsync(
            async token => await _dbContext.SaveChangesAsync(token), cancellationToken);
    }
}
