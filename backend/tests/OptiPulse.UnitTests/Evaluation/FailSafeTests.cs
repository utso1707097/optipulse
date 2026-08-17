using FluentAssertions;
using OptiPulse.Evaluation.Application;
using OptiPulse.Evaluation.Domain;
using OptiPulse.Evaluation.Infrastructure;
using Xunit;

namespace OptiPulse.UnitTests.Evaluation;

/// <summary>
/// FR-005 / SC-004: evaluation must keep serving correct last-known-good
/// decisions when the backing datastore is unavailable, with zero evaluation
/// failures. These tests target SnapshotStore + Evaluator, which is where the
/// fail-safe property actually lives: the hot path reads only the in-memory
/// snapshot and never touches the datastore, so a refresh failure degrades
/// freshness rather than availability.
/// </summary>
public sealed class FailSafeTests
{
    private static CompiledFlag Flag(string key, bool outcome, long version = 1) => new()
    {
        FlagKey = key,
        DefaultOutcome = outcome,
        KillSwitchEngaged = false,
        Version = version,
        TargetingRules = [],
        RolloutBasisPoints = null,
        RolloutSalt = null,
    };

    [Fact]
    public void Evaluation_ContinuesServingLastKnownGood_WhenNoFurtherRefreshesArrive()
    {
        // Simulates a datastore/Redis outage: the snapshot was loaded once, then
        // no further deltas ever arrive. Evaluation must keep succeeding.
        var store = new SnapshotStore();
        store.LoadInitial(new FlagSnapshot(5, DateTimeOffset.UtcNow, [Flag("feature.a", outcome: true, version: 5)]));
        var evaluator = new Evaluator(store);

        for (int i = 0; i < 1_000; i++)
        {
            var result = evaluator.Evaluate(new EvaluationContext("feature.a", $"user-{i}", null));
            result.Outcome.Should().BeTrue();
            result.Reason.Should().Be(EvaluationReason.Default);
            result.SnapshotVersion.Should().Be(5);
        }
    }

    [Fact]
    public void SnapshotStore_RetainsPreviousState_WhenADeltaIsStale()
    {
        // Out-of-order / duplicate delivery must never regress a flag to an older
        // state (invalidation-channel.md subscriber rule 1).
        var store = new SnapshotStore();
        store.LoadInitial(new FlagSnapshot(10, DateTimeOffset.UtcNow, [Flag("feature.a", outcome: true, version: 10)]));

        store.ApplyDelta(Flag("feature.a", outcome: false, version: 3), newVersion: 3);

        store.Current.TryGetFlag("feature.a", out var flag).Should().BeTrue();
        flag!.DefaultOutcome.Should().BeTrue("a stale delta must not overwrite newer state");
    }

    [Fact]
    public void SnapshotStore_AppliesFirstDeltaForANewFlag_EvenWhenGlobalVersionIsHigher()
    {
        // Regression guard: flags are versioned independently, so a new flag's
        // version-1 delta must apply even though an unrelated flag has already
        // pushed the snapshot-wide version far higher.
        var store = new SnapshotStore();
        store.LoadInitial(new FlagSnapshot(99, DateTimeOffset.UtcNow, [Flag("feature.a", outcome: true, version: 99)]));

        store.ApplyDelta(Flag("feature.b", outcome: true, version: 1), newVersion: 1);

        store.Current.TryGetFlag("feature.b", out var newFlag).Should().BeTrue(
            "a new flag's first delta must not be rejected by the global snapshot version");
        newFlag!.DefaultOutcome.Should().BeTrue();
    }

    [Fact]
    public void Evaluation_NeverThrows_OnAnEmptySnapshot()
    {
        // Worst case: the datastore was unavailable at startup, so no snapshot
        // ever loaded. Evaluation must still answer (safe default), not fail.
        var store = new SnapshotStore();
        var evaluator = new Evaluator(store);

        var act = () => evaluator.Evaluate(new EvaluationContext("anything", "user-1", null));

        act.Should().NotThrow();
        act().Reason.Should().Be(EvaluationReason.Unknown);
        act().Outcome.Should().BeFalse();
    }
}
