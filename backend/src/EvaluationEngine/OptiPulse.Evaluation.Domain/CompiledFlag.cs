namespace OptiPulse.Evaluation.Domain;

/// <summary>
/// Precomputed, allocation-free-to-read projection of a Flag for the evaluation
/// hot path (data-model.md, research R2). Ordered TargetingRules; first match
/// wins. RolloutBasisPoints is 0–10000; RolloutSalt feeds MurmurHash3 bucketing.
/// </summary>
public sealed class CompiledFlag
{
    public required string FlagKey { get; init; }
    public required bool DefaultOutcome { get; init; }
    public required bool KillSwitchEngaged { get; init; }
    public required long Version { get; init; }
    public required CompiledTargetingRule[] TargetingRules { get; init; }
    public int? RolloutBasisPoints { get; init; }
    public string? RolloutSalt { get; init; }
}
