using FluentAssertions;
using OptiPulse.Evaluation.Application;
using OptiPulse.Evaluation.Domain;
using Xunit;

namespace OptiPulse.UnitTests.Evaluation;

file sealed class FakeSnapshotStore(FlagSnapshot snapshot) : ISnapshotStore
{
    public FlagSnapshot Current { get; } = snapshot;
}

public sealed class EvaluatorTests
{
    private static CompiledFlag SimpleFlag(
        string key = "checkout.new-cta",
        bool defaultOutcome = false,
        bool killSwitch = false,
        CompiledTargetingRule[]? rules = null,
        int? rolloutBasisPoints = null,
        string? rolloutSalt = null) =>
        new()
        {
            FlagKey = key,
            DefaultOutcome = defaultOutcome,
            KillSwitchEngaged = killSwitch,
            Version = 1,
            TargetingRules = rules ?? [],
            RolloutBasisPoints = rolloutBasisPoints,
            RolloutSalt = rolloutSalt,
        };

    [Fact]
    public void Evaluate_UnknownFlag_ReturnsSafeDefaultWithoutError()
    {
        // FR-006: unknown flag requests never error; a safe default is returned.
        var store = new FakeSnapshotStore(new FlagSnapshot(1, DateTimeOffset.UtcNow, []));
        var evaluator = new Evaluator(store);

        var result = evaluator.Evaluate(new EvaluationContext("does-not-exist", "user-1", null));

        result.Outcome.Should().BeFalse();
        result.Reason.Should().Be(EvaluationReason.Unknown);
    }

    [Fact]
    public void Evaluate_KillSwitchEngaged_AlwaysReturnsDisabled_RegardlessOfTargetingOrRollout()
    {
        // FR-009: kill-switch takes precedence over every other signal.
        var flag = SimpleFlag(killSwitch: true, rolloutBasisPoints: 10_000); // 100% rollout
        var store = new FakeSnapshotStore(new FlagSnapshot(1, DateTimeOffset.UtcNow, [flag]));
        var evaluator = new Evaluator(store);

        var result = evaluator.Evaluate(new EvaluationContext(flag.FlagKey, "user-1", null));

        result.Outcome.Should().BeFalse();
        result.Reason.Should().Be(EvaluationReason.KillSwitch);
    }

    [Fact]
    public void Evaluate_TargetingRuleMatches_ReturnsRuleOutcome()
    {
        // FR-003: e.g. "country = US" -> enabled; falls through to default otherwise.
        var rule = new CompiledTargetingRule("country", CompiledTargetingOperator.Equals, ["US"], Outcome: true);
        var flag = SimpleFlag(defaultOutcome: false, rules: [rule]);
        var store = new FakeSnapshotStore(new FlagSnapshot(1, DateTimeOffset.UtcNow, [flag]));
        var evaluator = new Evaluator(store);

        var usResult = evaluator.Evaluate(new EvaluationContext(
            flag.FlagKey, "user-1", new Dictionary<string, string> { ["country"] = "US" }));
        var nonUsResult = evaluator.Evaluate(new EvaluationContext(
            flag.FlagKey, "user-2", new Dictionary<string, string> { ["country"] = "CA" }));

        usResult.Outcome.Should().BeTrue();
        usResult.Reason.Should().Be(EvaluationReason.TargetingMatch);

        nonUsResult.Outcome.Should().BeFalse(); // falls through to default
        nonUsResult.Reason.Should().Be(EvaluationReason.Default);
    }

    [Fact]
    public void Evaluate_SameContext_IsDeterministicAcrossRepeatedCalls()
    {
        // FR-001/SC-003: identical inputs + snapshot always yield the same result.
        var flag = SimpleFlag(rolloutBasisPoints: 5_000, rolloutSalt: "salt-1");
        var store = new FakeSnapshotStore(new FlagSnapshot(1, DateTimeOffset.UtcNow, [flag]));
        var evaluator = new Evaluator(store);
        var context = new EvaluationContext(flag.FlagKey, "user-42", null);

        var first = evaluator.Evaluate(context);
        for (int i = 0; i < 50; i++)
        {
            evaluator.Evaluate(context).Should().Be(first);
        }
    }

    [Fact]
    public void Evaluate_MissingContextKey_ResolvesToDefaultRolloutBucket_AndNeverThrows()
    {
        // Spec edge case: missing context key -> anonymous, resolves via default bucketing.
        var flag = SimpleFlag(rolloutBasisPoints: 5_000, rolloutSalt: "salt-1");
        var store = new FakeSnapshotStore(new FlagSnapshot(1, DateTimeOffset.UtcNow, [flag]));
        var evaluator = new Evaluator(store);

        var act = () => evaluator.Evaluate(new EvaluationContext(flag.FlagKey, ContextKey: null, null));

        act.Should().NotThrow();
        act().Reason.Should().Be(EvaluationReason.Rollout);
    }

    [Fact]
    public void Evaluate_NoTargetingOrRollout_ReturnsDefaultOutcome()
    {
        var flag = SimpleFlag(defaultOutcome: true);
        var store = new FakeSnapshotStore(new FlagSnapshot(1, DateTimeOffset.UtcNow, [flag]));
        var evaluator = new Evaluator(store);

        var result = evaluator.Evaluate(new EvaluationContext(flag.FlagKey, "user-1", null));

        result.Outcome.Should().BeTrue();
        result.Reason.Should().Be(EvaluationReason.Default);
    }
}
