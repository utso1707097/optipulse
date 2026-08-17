using BenchmarkDotNet.Attributes;
using OptiPulse.Evaluation.Application;
using OptiPulse.Evaluation.Domain;
using OptiPulse.Evaluation.Domain.Hashing;

namespace OptiPulse.Evaluation.Benchmarks;

/// <summary>
/// Constitution Principle II gate (tasks.md T018): proves p99 &lt; 5ms and ZERO
/// steady-state allocations on the evaluation hot path. MemoryDiagnoser reports
/// allocated bytes per operation — the gate asserts 0 B.
/// </summary>
[MemoryDiagnoser]
public class EvaluationBenchmarks
{
    private Evaluator _evaluator = null!;
    private EvaluationContext _rolloutContext;
    private EvaluationContext _targetingContext;
    private EvaluationContext _killSwitchContext;
    private EvaluationContext _unknownContext;

    private sealed class FixedSnapshotStore(FlagSnapshot snapshot) : ISnapshotStore
    {
        public FlagSnapshot Current { get; } = snapshot;
    }

    [GlobalSetup]
    public void Setup()
    {
        var rolloutFlag = new CompiledFlag
        {
            FlagKey = "checkout.rollout",
            DefaultOutcome = false,
            KillSwitchEngaged = false,
            Version = 1,
            TargetingRules = [],
            RolloutBasisPoints = 5_000,
            RolloutSalt = "salt-1",
        };

        var targetingFlag = new CompiledFlag
        {
            FlagKey = "checkout.targeting",
            DefaultOutcome = false,
            KillSwitchEngaged = false,
            Version = 1,
            TargetingRules =
            [
                new CompiledTargetingRule("country", CompiledTargetingOperator.Equals, ["US"], Outcome: true),
                new CompiledTargetingRule("plan", CompiledTargetingOperator.In, ["pro", "enterprise"], Outcome: true),
            ],
            RolloutBasisPoints = null,
            RolloutSalt = null,
        };

        var killSwitchFlag = new CompiledFlag
        {
            FlagKey = "checkout.killed",
            DefaultOutcome = true,
            KillSwitchEngaged = true,
            Version = 1,
            TargetingRules = [],
            RolloutBasisPoints = 10_000,
            RolloutSalt = "salt-1",
        };

        var snapshot = new FlagSnapshot(1, DateTimeOffset.UtcNow, [rolloutFlag, targetingFlag, killSwitchFlag]);
        _evaluator = new Evaluator(new FixedSnapshotStore(snapshot));

        _rolloutContext = new EvaluationContext("checkout.rollout", "user-12345", null);
        _targetingContext = new EvaluationContext("checkout.targeting", "user-12345",
            new Dictionary<string, string> { ["country"] = "US", ["plan"] = "pro" });
        _killSwitchContext = new EvaluationContext("checkout.killed", "user-12345", null);
        _unknownContext = new EvaluationContext("does.not.exist", "user-12345", null);
    }

    [Benchmark(Description = "Evaluate: percentage rollout (MurmurHash3 bucketing)")]
    public EvaluationResult EvaluateRollout() => _evaluator.Evaluate(_rolloutContext);

    [Benchmark(Description = "Evaluate: targeting rule match")]
    public EvaluationResult EvaluateTargeting() => _evaluator.Evaluate(_targetingContext);

    [Benchmark(Description = "Evaluate: kill-switch (short-circuit)")]
    public EvaluationResult EvaluateKillSwitch() => _evaluator.Evaluate(_killSwitchContext);

    [Benchmark(Description = "Evaluate: unknown flag (safe default)")]
    public EvaluationResult EvaluateUnknown() => _evaluator.Evaluate(_unknownContext);

    [Benchmark(Description = "MurmurHash3.ComputeBucket (raw bucketing)")]
    public int ComputeBucket() => MurmurHash3.ComputeBucket("checkout.rollout", "salt-1", "user-12345");
}
