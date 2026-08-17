using OptiPulse.Audit.Domain;
using OptiPulse.SharedKernel;

namespace OptiPulse.Audit.Application;

/// <summary>Append-only audit port (FR-019). No update/delete methods exist by design.</summary>
public interface IAuditLog
{
    Task AppendAsync(
        ActorReference actor,
        AuditChangeType changeType,
        Guid targetId,
        string? beforeStateJson = null,
        string? afterStateJson = null,
        CancellationToken cancellationToken = default);
}
