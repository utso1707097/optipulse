namespace OptiPulse.Evaluation.Domain;

/// <summary>Outcome of a single flag evaluation (data-model.md, contracts/evaluation-api.md).</summary>
public readonly record struct EvaluationResult(
    bool Outcome,
    string? VariantKey,
    EvaluationReason Reason,
    long SnapshotVersion);

/// <summary>Explains why an evaluation resolved the way it did (observability).</summary>
public enum EvaluationReason
{
    Default,
    TargetingMatch,
    Rollout,
    KillSwitch,
    Unknown,
    FailSafe,
}
