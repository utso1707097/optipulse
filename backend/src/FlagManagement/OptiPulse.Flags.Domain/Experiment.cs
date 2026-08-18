using OptiPulse.SharedKernel;

namespace OptiPulse.Flags.Domain;

/// <summary>
/// Experiment aggregate root (data-model.md): an A/B/n test over a Flag.
///
/// Weights are basis points (0–10000), not percentages, for the same reason the rollout is:
/// integer arithmetic against MurmurHash3's bucket space keeps assignment exact. Percentages
/// would force rounding, and a "50/50" split that silently became 4999/5001 buckets is the kind
/// of skew that quietly invalidates a result nobody thinks to question.
/// </summary>
public sealed class Experiment
{
    public const int TotalBasisPoints = 10_000;

    public Guid Id { get; private set; }
    public Guid FlagId { get; private set; }
    public string FlagKey { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public ExperimentStatus Status { get; private set; }
    public string? ConversionGoal { get; private set; }
    public long Version { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public List<Variant> Variants { get; private set; } = [];

    // EF materialization constructor — owned collections cannot bind through a parameterized one.
    private Experiment() { }

    public static Result<Experiment> Create(
        Guid flagId,
        string flagKey,
        string name,
        IReadOnlyList<Variant> variants,
        string? conversionGoal,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("Experiment.Name.Required", "Experiment name is required.");

        var validated = ValidateVariants(variants);
        if (validated.IsFailure)
            return validated.Error;

        return new Experiment
        {
            Id = Guid.NewGuid(),
            FlagId = flagId,
            FlagKey = flagKey,
            Name = name.Trim(),
            Status = ExperimentStatus.Draft,
            ConversionGoal = string.IsNullOrWhiteSpace(conversionGoal) ? null : conversionGoal.Trim(),
            Version = 1,
            Variants = [.. variants],
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    /// <summary>
    /// Replaces the variant set. Note what is NOT re-randomised: assignment is derived from the
    /// context key and the flag's salt, so a context that already saw variant "b" keeps seeing
    /// "b" as long as "b" still exists and its bucket range still covers them. Changing weights
    /// mid-flight therefore affects newly-bucketed contexts, not existing ones (FR-004 edge case).
    /// </summary>
    public Result UpdateVariants(IReadOnlyList<Variant> variants, DateTimeOffset now)
    {
        if (Status == ExperimentStatus.Concluded)
            return Error.Conflict("Experiment.Update.Concluded", "A concluded experiment cannot be edited.");

        var validated = ValidateVariants(variants);
        if (validated.IsFailure)
            return validated.Error;

        Variants = [.. variants];
        Version++;
        UpdatedAt = now;
        return Result.Success();
    }

    public Result Start(DateTimeOffset now)
    {
        if (Status != ExperimentStatus.Draft)
            return Error.Conflict("Experiment.Start.InvalidState", "Only a Draft experiment can start.");

        Status = ExperimentStatus.Running;
        Version++;
        UpdatedAt = now;
        return Result.Success();
    }

    public Result Conclude(DateTimeOffset now)
    {
        if (Status != ExperimentStatus.Running)
            return Error.Conflict("Experiment.Conclude.InvalidState", "Only a Running experiment can be concluded.");

        Status = ExperimentStatus.Concluded;
        Version++;
        UpdatedAt = now;
        return Result.Success();
    }

    private static Result ValidateVariants(IReadOnlyList<Variant> variants)
    {
        if (variants.Count < 2)
            return Error.Validation(
                "Experiment.Variants.TooFew", "An experiment needs at least 2 variants.");

        var duplicateKey = variants
            .GroupBy(v => v.Key, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicateKey is not null)
            return Error.Validation(
                "Experiment.Variants.DuplicateKey", $"Variant key '{duplicateKey.Key}' is repeated.");

        if (variants.Any(v => v.WeightBasisPoints < 0 || v.WeightBasisPoints > TotalBasisPoints))
            return Error.Validation(
                "Experiment.Variants.WeightOutOfRange", "Variant weights must be between 0 and 10000 basis points.");

        var total = variants.Sum(v => v.WeightBasisPoints);
        if (total != TotalBasisPoints)
            return Error.Validation(
                "Experiment.Variants.WeightSum",
                $"Variant weights must sum to 100% (10000 basis points); got {total}.");

        return Result.Success();
    }
}

/// <summary>Entity within <see cref="Experiment"/>. Weight is basis points, not percent.</summary>
public sealed record Variant(string Key, int WeightBasisPoints, Guid? MicroCopyCandidateId = null)
{
    public static Result<Variant> FromPercentage(string key, int percentage)
    {
        if (string.IsNullOrWhiteSpace(key))
            return Error.Validation("Variant.Key.Required", "Variant key is required.");
        if (percentage is < 0 or > 100)
            return Error.Validation("Variant.Weight.OutOfRange", "Variant percentage must be 0-100.");

        return new Variant(key.Trim(), percentage * 100);
    }
}

public enum ExperimentStatus
{
    Draft,
    Running,
    Concluded,
}
