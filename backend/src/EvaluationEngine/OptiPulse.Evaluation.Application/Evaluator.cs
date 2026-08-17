using OptiPulse.Evaluation.Domain;
using OptiPulse.Evaluation.Domain.Hashing;

namespace OptiPulse.Evaluation.Application;

/// <summary>
/// Core evaluation logic (FR-001–006). No I/O, no allocation beyond the returned
/// struct — reads the current snapshot reference once and never touches EF Core,
/// Redis, or any external dependency (Principle II: the hot path stays in-memory).
/// </summary>
public sealed class Evaluator(ISnapshotStore snapshotStore) : IEvaluator
{
    public EvaluationResult Evaluate(EvaluationContext context)
    {
        var snapshot = snapshotStore.Current;

        if (!snapshot.TryGetFlag(context.FlagKey, out var flag) || flag is null)
            return new EvaluationResult(false, null, EvaluationReason.Unknown, snapshot.Version);

        // Kill-switch takes precedence over every other signal (FR-009, fail-safe "off").
        if (flag.KillSwitchEngaged)
            return new EvaluationResult(false, null, EvaluationReason.KillSwitch, snapshot.Version);

        foreach (var rule in flag.TargetingRules)
        {
            if (rule.Matches(context.Attributes))
                return new EvaluationResult(rule.Outcome, null, EvaluationReason.TargetingMatch, snapshot.Version);
        }

        if (flag.RolloutBasisPoints is { } basisPoints)
        {
            // Missing context key resolves to the default and is treated as
            // anonymous (spec edge case) — never crashes, never blocks evaluation.
            string bucketingKey = context.ContextKey ?? "__anonymous__";
            int bucket = MurmurHash3.ComputeBucket(context.FlagKey, flag.RolloutSalt ?? string.Empty, bucketingKey);
            bool inRollout = bucket < basisPoints;
            return new EvaluationResult(inRollout, null, EvaluationReason.Rollout, snapshot.Version);
        }

        return new EvaluationResult(flag.DefaultOutcome, null, EvaluationReason.Default, snapshot.Version);
    }
}
