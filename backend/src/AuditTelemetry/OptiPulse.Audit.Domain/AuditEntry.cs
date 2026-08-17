using OptiPulse.SharedKernel;

namespace OptiPulse.Audit.Domain;

/// <summary>
/// Immutable, append-only audit record (data-model.md, FR-019, SC-006). No
/// mutation methods by design — the data layer additionally grants no
/// UPDATE/DELETE on the backing table (enforced in Infrastructure).
/// </summary>
public sealed class AuditEntry
{
    public Guid Id { get; }
    public DateTimeOffset Timestamp { get; }
    public ActorReference Actor { get; }
    public AuditChangeType ChangeType { get; }
    public Guid TargetId { get; }
    public string? BeforeStateJson { get; }
    public string? AfterStateJson { get; }

    /// <summary>EF Core materialization constructor — see Flag.cs for why this
    /// pattern is needed (owned/complex navigations can't bind via a parameterized
    /// constructor). All members here are value types or nullable, so no explicit
    /// initialization is required.</summary>
    private AuditEntry()
    {
    }

    private AuditEntry(
        Guid id, DateTimeOffset timestamp, ActorReference actor, AuditChangeType changeType,
        Guid targetId, string? beforeStateJson, string? afterStateJson)
    {
        Id = id;
        Timestamp = timestamp;
        Actor = actor;
        ChangeType = changeType;
        TargetId = targetId;
        BeforeStateJson = beforeStateJson;
        AfterStateJson = afterStateJson;
    }

    public static AuditEntry Create(
        ActorReference actor,
        AuditChangeType changeType,
        Guid targetId,
        DateTimeOffset now,
        string? beforeStateJson = null,
        string? afterStateJson = null) =>
        new(Guid.NewGuid(), now, actor, changeType, targetId, beforeStateJson, afterStateJson);

    public static AuditEntry FromPersistence(
        Guid id, DateTimeOffset timestamp, ActorReference actor, AuditChangeType changeType,
        Guid targetId, string? beforeStateJson, string? afterStateJson) =>
        new(id, timestamp, actor, changeType, targetId, beforeStateJson, afterStateJson);
}

public enum AuditChangeType
{
    FlagCreated,
    FlagUpdated,
    KillSwitchEngaged,
    KillSwitchReleased,
    ExperimentChanged,
    CopyGenerated,
    CopyApproved,
    CopyRejected,
    RoleDenied,
    LoginSucceeded,
    LoginFailed,
}
