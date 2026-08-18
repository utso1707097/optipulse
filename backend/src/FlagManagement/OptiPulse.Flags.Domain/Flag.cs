using OptiPulse.SharedKernel;

namespace OptiPulse.Flags.Domain;

/// <summary>
/// Flag aggregate root (data-model.md).
///
/// Every state change bumps <see cref="Version"/>, which serves three jobs at once: EF's
/// optimistic-concurrency token (FR-011, so a concurrent edit 409s instead of silently
/// overwriting), the ordering key the evaluation snapshot uses to reject stale invalidation
/// deltas, and the identifier for recoverable history (FR-010).
/// </summary>
public sealed class Flag
{
    public Guid Id { get; private set; }
    public string Key { get; private set; }
    public string Name { get; private set; }
    public bool DefaultOutcome { get; private set; }
    public FlagStatus Status { get; private set; }
    public bool KillSwitchEngaged { get; private set; }
    public long Version { get; private set; }
    public List<TargetingRule> TargetingRules { get; private set; }
    public Rollout? Rollout { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>EF Core materialization constructor. Owned/complex navigations
    /// (Rollout) cannot bind through a parameterized constructor, so EF prefers
    /// this parameterless one and sets all properties via their private setters.
    /// Not for domain use — use <see cref="Create"/> or <see cref="FromPersistence"/>.</summary>
    private Flag()
    {
        Key = string.Empty;
        Name = string.Empty;
        TargetingRules = [];
    }

    private Flag(
        Guid id,
        string key,
        string name,
        bool defaultOutcome,
        FlagStatus status,
        bool killSwitchEngaged,
        long version,
        List<TargetingRule> targetingRules,
        Rollout? rollout,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        Id = id;
        Key = key;
        Name = name;
        DefaultOutcome = defaultOutcome;
        Status = status;
        KillSwitchEngaged = killSwitchEngaged;
        Version = version;
        TargetingRules = targetingRules;
        Rollout = rollout;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Result<Flag> Create(
        string key,
        string name,
        bool defaultOutcome,
        DateTimeOffset now,
        List<TargetingRule>? targetingRules = null,
        Rollout? rollout = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Error.Validation("Flag.Key.Required", "Flag key is required.");
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("Flag.Name.Required", "Flag name is required.");

        return new Flag(
            Guid.NewGuid(),
            key,
            name,
            defaultOutcome,
            FlagStatus.Draft,
            killSwitchEngaged: false,
            version: 1,
            targetingRules ?? [],
            rollout,
            now,
            now);
    }

    /// <summary>Reconstitutes a Flag from persisted state (EF Core materialization path).</summary>
    public static Flag FromPersistence(
        Guid id, string key, string name, bool defaultOutcome, FlagStatus status,
        bool killSwitchEngaged, long version, List<TargetingRule> targetingRules,
        Rollout? rollout, DateTimeOffset createdAt, DateTimeOffset updatedAt) =>
        new(id, key, name, defaultOutcome, status, killSwitchEngaged, version,
            targetingRules, rollout, createdAt, updatedAt);

    /// <summary>
    /// Edits the configurable fields. Archived flags are frozen — an edit to something no
    /// longer served would be silently meaningless, so it is refused rather than accepted.
    /// </summary>
    public Result Update(
        string name,
        bool defaultOutcome,
        List<TargetingRule> targetingRules,
        Rollout? rollout,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("Flag.Name.Required", "Flag name is required.");
        if (Status == FlagStatus.Archived)
            return Error.Conflict("Flag.Update.Archived", "An archived flag cannot be edited.");

        Name = name;
        DefaultOutcome = defaultOutcome;
        TargetingRules = targetingRules;
        Rollout = rollout;
        Version++;
        UpdatedAt = now;
        return Result.Success();
    }

    /// <summary>
    /// Archive is terminal: Draft→Active→Archived, never back. Reviving a flag by key would
    /// let an old key silently acquire new meaning while consumers still reference it.
    /// </summary>
    public Result Archive(DateTimeOffset now)
    {
        if (Status == FlagStatus.Archived)
            return Error.Conflict("Flag.Archive.InvalidState", "Flag is already archived.");

        Status = FlagStatus.Archived;
        // An archived flag must not stay kill-switched: the kill-switch is an operational
        // override on a live flag, and leaving it engaged on a dead one is misleading state.
        KillSwitchEngaged = false;
        Version++;
        UpdatedAt = now;
        return Result.Success();
    }

    public Result Activate(DateTimeOffset now)
    {
        if (Status != FlagStatus.Draft)
            return Error.Conflict("Flag.Activate.InvalidState", "Only a Draft flag can be activated.");
        Status = FlagStatus.Active;
        Version++;
        UpdatedAt = now;
        return Result.Success();
    }

    public Result EngageKillSwitch(DateTimeOffset now)
    {
        if (Status != FlagStatus.Active)
            return Error.Conflict("Flag.KillSwitch.InvalidState", "Only an Active flag can be kill-switched.");
        KillSwitchEngaged = true;
        Version++;
        UpdatedAt = now;
        return Result.Success();
    }

    public Result ReleaseKillSwitch(DateTimeOffset now)
    {
        if (!KillSwitchEngaged)
            return Result.Success();
        KillSwitchEngaged = false;
        Version++;
        UpdatedAt = now;
        return Result.Success();
    }
}

public enum FlagStatus
{
    Draft,
    Active,
    Archived,
}
