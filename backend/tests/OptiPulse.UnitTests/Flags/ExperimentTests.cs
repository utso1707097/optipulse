using FluentAssertions;
using OptiPulse.Flags.Domain;
using Xunit;

namespace OptiPulse.UnitTests.Flags;

/// <summary>
/// T047 — the weights-sum-to-100% invariant is the whole point of the aggregate. A split that
/// does not total 100% means some share of traffic falls into no arm at all, and the resulting
/// experiment looks fine while quietly measuring the wrong denominator.
/// </summary>
public sealed class ExperimentTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static Variant V(string key, int percent) => Variant.FromPercentage(key, percent).Value;

    [Fact]
    public void Create_WithTwoFiftyPercentVariants_Succeeds_InDraft()
    {
        var result = Experiment.Create(
            Guid.NewGuid(), "checkout", "CTA test", [V("control", 50), V("b", 50)], "purchase", Now);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(ExperimentStatus.Draft);
        result.Value.Variants.Sum(v => v.WeightBasisPoints).Should().Be(Experiment.TotalBasisPoints);
    }

    [Theory]
    [InlineData(50, 40)]   // under 100
    [InlineData(60, 50)]   // over 100
    [InlineData(0, 0)]     // no traffic at all
    public void Create_WhenWeightsDoNotSumTo100_Fails(int first, int second)
    {
        var result = Experiment.Create(
            Guid.NewGuid(), "checkout", "Bad split", [V("a", first), V("b", second)], null, Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Experiment.Variants.WeightSum");
    }

    [Fact]
    public void Create_WithFewerThanTwoVariants_Fails()
    {
        var result = Experiment.Create(
            Guid.NewGuid(), "checkout", "Not a test", [V("only", 100)], null, Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Experiment.Variants.TooFew");
    }

    [Fact]
    public void Create_WithDuplicateVariantKeys_Fails()
    {
        // Two arms with the same key cannot be told apart in exposure telemetry, so the results
        // would be unattributable even though the weights are valid.
        var result = Experiment.Create(
            Guid.NewGuid(), "checkout", "Dupes", [V("a", 50), V("a", 50)], null, Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Experiment.Variants.DuplicateKey");
    }

    [Fact]
    public void ThreeWaySplit_OfThirtyThreeThirtyThreeThirtyFour_Sums_Exactly()
    {
        // Basis points exist so an uneven split is exact rather than rounded.
        var result = Experiment.Create(
            Guid.NewGuid(), "checkout", "3-way", [V("a", 33), V("b", 33), V("c", 34)], null, Now);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Lifecycle_IsDraftThenRunningThenConcluded_AndCannotSkipOrRepeat()
    {
        var experiment = Experiment.Create(
            Guid.NewGuid(), "checkout", "Lifecycle", [V("a", 50), V("b", 50)], null, Now).Value;

        experiment.Conclude(Now).IsFailure.Should().BeTrue("a Draft experiment has nothing to conclude");
        experiment.Start(Now).IsSuccess.Should().BeTrue();
        experiment.Start(Now).IsFailure.Should().BeTrue("it is already running");
        experiment.Conclude(Now).IsSuccess.Should().BeTrue();
        experiment.Status.Should().Be(ExperimentStatus.Concluded);
    }

    [Fact]
    public void UpdateVariants_OnAConcludedExperiment_IsRefused()
    {
        // Editing the arms of a finished experiment would rewrite the meaning of results already
        // reported against it.
        var experiment = Experiment.Create(
            Guid.NewGuid(), "checkout", "Done", [V("a", 50), V("b", 50)], null, Now).Value;
        experiment.Start(Now);
        experiment.Conclude(Now);

        var result = experiment.UpdateVariants([V("a", 70), V("b", 30)], Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Experiment.Update.Concluded");
    }

    [Fact]
    public void UpdateVariants_BumpsVersion_ForOptimisticConcurrency()
    {
        var experiment = Experiment.Create(
            Guid.NewGuid(), "checkout", "Reweight", [V("a", 50), V("b", 50)], null, Now).Value;
        var before = experiment.Version;

        experiment.UpdateVariants([V("a", 70), V("b", 30)], Now).IsSuccess.Should().BeTrue();

        experiment.Version.Should().Be(before + 1);
    }
}
