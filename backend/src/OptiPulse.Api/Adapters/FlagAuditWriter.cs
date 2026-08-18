using OptiPulse.Audit.Application;
using OptiPulse.Audit.Domain;
using OptiPulse.Flags.Application;
using OptiPulse.SharedKernel;

namespace OptiPulse.Api.Adapters;

/// <summary>
/// Composition-root adapter joining Flag Management's audit port to the Audit &amp; Telemetry
/// context's log. This is the only place the two contexts meet, which is what keeps their
/// isolation intact while still producing one unified trail.
/// </summary>
public sealed class FlagAuditWriter(IAuditLog auditLog) : IFlagAuditWriter
{
    public Task RecordAsync(
        ActorReference actor,
        FlagAuditAction action,
        Guid flagId,
        string? beforeStateJson,
        string? afterStateJson,
        CancellationToken cancellationToken = default) =>
        auditLog.AppendAsync(actor, Map(action), flagId, beforeStateJson, afterStateJson, cancellationToken);

    private static AuditChangeType Map(FlagAuditAction action) => action switch
    {
        FlagAuditAction.Created => AuditChangeType.FlagCreated,
        FlagAuditAction.Updated => AuditChangeType.FlagUpdated,
        FlagAuditAction.KillSwitchEngaged => AuditChangeType.KillSwitchEngaged,
        FlagAuditAction.KillSwitchReleased => AuditChangeType.KillSwitchReleased,
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "Unmapped flag audit action."),
    };
}
