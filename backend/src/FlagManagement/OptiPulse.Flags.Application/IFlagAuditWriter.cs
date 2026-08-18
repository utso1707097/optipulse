using OptiPulse.SharedKernel;

namespace OptiPulse.Flags.Application;

/// <summary>
/// Audit port owned by Flag Management (Principle I: the inner layer defines the interface).
///
/// Flag Management deliberately does NOT reference the Audit &amp; Telemetry context. Taking a
/// direct dependency on IAuditLog to save writing this port would couple two bounded contexts,
/// which is the isolation T090's boundary tests exist to enforce. The composition root owns the
/// adapter, because wiring two contexts together is exactly its job.
/// </summary>
public interface IFlagAuditWriter
{
    Task RecordAsync(
        ActorReference actor,
        FlagAuditAction action,
        Guid flagId,
        string? beforeStateJson,
        string? afterStateJson,
        CancellationToken cancellationToken = default);
}

public enum FlagAuditAction
{
    Created,
    Updated,
    KillSwitchEngaged,
    KillSwitchReleased,
    ExperimentChanged,
}
