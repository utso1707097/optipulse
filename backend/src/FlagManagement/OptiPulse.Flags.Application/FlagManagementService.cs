using OptiPulse.Flags.Domain;
using OptiPulse.SharedKernel;

namespace OptiPulse.Flags.Application;

/// <summary>
/// Write-side use cases for the Flag aggregate (T050).
///
/// Every mutation follows the same order, and the order matters:
///   1. load + apply the domain rule (the aggregate refuses invalid transitions)
///   2. commit — a concurrency conflict returns a Result, never a silent overwrite
///   3. audit the change (FR-A06: attributed to the acting user and role)
///   4. publish invalidation LAST, so no node is ever told about a version that failed to save
/// </summary>
public sealed class FlagManagementService(
    IFlagRepository repository,
    IInvalidationPublisher publisher,
    IFlagAuditWriter auditWriter,
    TimeProvider timeProvider)
{
    public Task<IReadOnlyList<Flag>> ListAsync(CancellationToken ct = default) =>
        repository.ListAsync(ct);

    public Task<Flag?> GetAsync(string key, CancellationToken ct = default) =>
        repository.GetByKeyAsync(key, ct);

    public async Task<Result<Flag>> CreateAsync(
        ActorReference actor,
        string key,
        string name,
        bool defaultOutcome,
        List<TargetingRule>? targetingRules,
        Rollout? rollout,
        CancellationToken ct = default)
    {
        if (await repository.ExistsAsync(key, ct))
            return Error.Conflict("Flag.KeyExists", $"A flag with key '{key}' already exists.");

        var created = Flag.Create(key, name, defaultOutcome, timeProvider.GetUtcNow(), targetingRules, rollout);
        if (created.IsFailure)
            return created.Error;

        var flag = created.Value;
        await repository.AddAsync(flag, ct);

        var saved = await repository.SaveChangesAsync(ct);
        if (saved.IsFailure)
            return saved.Error;

        await auditWriter.RecordAsync(
            actor, FlagAuditAction.Created, flag.Id, null, Describe(flag), ct);

        // A new flag starts as Draft and is not served, but publishing keeps every node's view
        // consistent from creation rather than only from first activation.
        await publisher.PublishFlagChangedAsync(flag, ct);
        return flag;
    }

    public async Task<Result<Flag>> UpdateAsync(
        ActorReference actor,
        string key,
        long expectedVersion,
        string name,
        bool defaultOutcome,
        List<TargetingRule> targetingRules,
        Rollout? rollout,
        CancellationToken ct = default)
    {
        var flag = await repository.GetByKeyAsync(key, ct);
        if (flag is null)
            return Error.NotFound("Flag.NotFound", $"Flag '{key}' was not found.");

        // Check the caller's If-Match BEFORE mutating: the caller edited a specific version, and
        // applying their edit on top of someone else's newer one is the lost update FR-011 exists
        // to prevent. EF's token catches the race at commit; this catches the stale read earlier
        // and reports it with the version the caller actually needs.
        if (flag.Version != expectedVersion)
            return Error.Conflict(
                "Flag.VersionMismatch",
                $"Flag '{key}' is at version {flag.Version}, not {expectedVersion}. Re-read and retry.");

        var before = Describe(flag);

        var updated = flag.Update(name, defaultOutcome, targetingRules, rollout, timeProvider.GetUtcNow());
        if (updated.IsFailure)
            return updated.Error;

        var saved = await repository.SaveChangesAsync(ct);
        if (saved.IsFailure)
            return saved.Error;

        await auditWriter.RecordAsync(
            actor, FlagAuditAction.Updated, flag.Id, before, Describe(flag), ct);

        await publisher.PublishFlagChangedAsync(flag, ct);
        return flag;
    }

    public async Task<Result<Flag>> ChangeStatusAsync(
        ActorReference actor, string key, FlagStatus target, CancellationToken ct = default)
    {
        var flag = await repository.GetByKeyAsync(key, ct);
        if (flag is null)
            return Error.NotFound("Flag.NotFound", $"Flag '{key}' was not found.");

        var before = Describe(flag);
        var now = timeProvider.GetUtcNow();

        var transition = target switch
        {
            FlagStatus.Active => flag.Activate(now),
            FlagStatus.Archived => flag.Archive(now),
            _ => Result.Failure(Error.Validation(
                "Flag.Status.Unsupported", "Only Active and Archived are reachable transitions.")),
        };

        if (transition.IsFailure)
            return transition.Error;

        var saved = await repository.SaveChangesAsync(ct);
        if (saved.IsFailure)
            return saved.Error;

        await auditWriter.RecordAsync(
            actor, FlagAuditAction.Updated, flag.Id, before, Describe(flag), ct);

        await publisher.PublishFlagChangedAsync(flag, ct);
        return flag;
    }

    /// <summary>
    /// The emergency override (FR-009, SC-002 &lt;100ms). Admin-only at the endpoint: a
    /// kill-switch is an operational action, not an authoring one.
    /// </summary>
    public async Task<Result<Flag>> SetKillSwitchAsync(
        ActorReference actor, string key, bool engaged, CancellationToken ct = default)
    {
        var flag = await repository.GetByKeyAsync(key, ct);
        if (flag is null)
            return Error.NotFound("Flag.NotFound", $"Flag '{key}' was not found.");

        var before = Describe(flag);
        var now = timeProvider.GetUtcNow();

        var transition = engaged ? flag.EngageKillSwitch(now) : flag.ReleaseKillSwitch(now);
        if (transition.IsFailure)
            return transition.Error;

        var saved = await repository.SaveChangesAsync(ct);
        if (saved.IsFailure)
            return saved.Error;

        await auditWriter.RecordAsync(
            actor,
            engaged ? FlagAuditAction.KillSwitchEngaged : FlagAuditAction.KillSwitchReleased,
            flag.Id, before, Describe(flag), ct);

        await publisher.PublishKillSwitchAsync(flag, ct);
        return flag;
    }

    /// <summary>Compact before/after snapshot for the audit trail — enough to reconstruct what
    /// changed without duplicating the whole aggregate into every entry.</summary>
    private static string Describe(Flag flag) =>
        $$"""{"key":"{{flag.Key}}","name":"{{flag.Name}}","status":"{{flag.Status}}","defaultOutcome":{{(flag.DefaultOutcome ? "true" : "false")}},"killSwitchEngaged":{{(flag.KillSwitchEngaged ? "true" : "false")}},"version":{{flag.Version}}}""";
}
