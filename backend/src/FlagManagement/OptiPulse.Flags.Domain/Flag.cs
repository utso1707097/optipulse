using OptiPulse.SharedKernel;

namespace OptiPulse.Flags.Domain;

/// <summary>
/// Flag aggregate root (data-model.md). NOTE: this MVP slice models the aggregate
/// and its persistence shape so the Evaluation Engine can bootstrap from real
/// data; the authoring/write API (create/edit/kill-switch endpoints) is Phase 5
/// (US3) and not implemented here.
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
