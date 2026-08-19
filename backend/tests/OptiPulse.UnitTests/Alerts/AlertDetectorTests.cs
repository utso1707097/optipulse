using FluentAssertions;
using OptiPulse.Audit.Application;
using OptiPulse.Audit.Domain;

namespace OptiPulse.UnitTests.Alerts;

/// <summary>
/// The detector's rules (T068). Unit-level because the rules are pure: the failure mode of an
/// alerting system is not usually a crash, it is alerting too much or not at all — and both are
/// silent, so the thresholds need direct tests rather than being inferred from an integration
/// run that happened to look reasonable.
/// </summary>
public sealed class AlertDetectorTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 19, 12, 0, 0, TimeSpan.Zero);
    private static readonly AlertThresholds Defaults = new();

    [Fact]
    public void KillSwitchEngaged_IsCritical_AndNamesTheActor()
    {
        var alert = AlertDetector.KillSwitchChanged("checkout.new-flow", engaged: true, "ada", Now);

        alert.Kind.Should().Be(AlertKind.KillSwitchChanged);
        alert.Severity.Should().Be(AlertSeverity.Critical);
        alert.FlagKey.Should().Be("checkout.new-flow");
        alert.Detail.Should().Contain("ada");
    }

    [Fact]
    public void KillSwitchReleased_IsOnlyAWarning()
    {
        // Both are worth knowing, but only one of them means a feature just stopped serving for
        // everybody. Equal severities would make the critical one unremarkable.
        var alert = AlertDetector.KillSwitchChanged("checkout.new-flow", engaged: false, "ada", Now);

        alert.Severity.Should().Be(AlertSeverity.Warning);
    }

    [Fact]
    public void KillSwitchChanges_AreNotCollapsedIntoOneAlert()
    {
        // An engage/release/engage sequence is flapping, and flapping is the signal. Time-bucket
        // deduping would hide it behind a single alert.
        var first = AlertDetector.KillSwitchChanged("f", engaged: true, "ada", Now);
        var second = AlertDetector.KillSwitchChanged("f", engaged: false, "ada", Now.AddSeconds(3));

        second.DedupeKey.Should().NotBe(first.DedupeKey);
    }

    [Theory]
    [InlineData(99, 3, "sample below the minimum is not reported at all")]
    [InlineData(1_000, 40, "4% is under the 5% threshold")]
    public void ErrorRateSpike_StaysQuiet(int total, int errors, string because)
    {
        AlertDetector.ErrorRateSpike("f", errors, total, Now, Defaults)
            .Should().BeNull(because);
    }

    [Fact]
    public void ErrorRateSpike_OnATinySample_IsNotReported()
    {
        // Two errors out of three requests is 67% and means nothing. A threshold with no minimum
        // sample fires constantly during quiet periods, which is how people learn to ignore it.
        AlertDetector.ErrorRateSpike("f", errorCount: 2, totalCount: 3, Now, Defaults)
            .Should().BeNull();
    }

    [Fact]
    public void ErrorRateSpike_AboveThreshold_IsCritical()
    {
        var alert = AlertDetector.ErrorRateSpike("f", errorCount: 120, totalCount: 1_000, Now, Defaults);

        alert.Should().NotBeNull();
        alert!.Severity.Should().Be(AlertSeverity.Critical);
        alert.Kind.Should().Be(AlertKind.ErrorRateSpike);
    }

    [Fact]
    public void AStandingCondition_ProducesOneKeyPerWindow()
    {
        // The dedupe key is what stops a ten-minute spike raising ten identical alerts. An
        // operator whose phone buzzes ten times for one incident stops reading them.
        var early = AlertDetector.ErrorRateSpike("f", 120, 1_000, Now, Defaults)!;
        var sameWindow = AlertDetector.ErrorRateSpike("f", 130, 1_000, Now.AddMinutes(5), Defaults)!;
        var nextWindow = AlertDetector.ErrorRateSpike("f", 130, 1_000, Now.AddMinutes(20), Defaults)!;

        sameWindow.DedupeKey.Should().Be(early.DedupeKey);
        nextWindow.DedupeKey.Should().NotBe(early.DedupeKey);
    }

    [Fact]
    public void AnomalousExposure_WithinTolerance_IsNotReported()
    {
        // 52% observed against a 50% target is ordinary sampling noise.
        AlertDetector.AnomalousExposure("f", "B", 5_000, 5_200, 10_000, Now, Defaults)
            .Should().BeNull();
    }

    [Fact]
    public void AnomalousExposure_BeyondTolerance_IsReported()
    {
        var alert = AlertDetector.AnomalousExposure("f", "B", 5_000, 8_000, 10_000, Now, Defaults);

        alert.Should().NotBeNull();
        alert!.Kind.Should().Be(AlertKind.AnomalousExposure);
        alert.Detail.Should().Contain("80.0%");
        alert.Detail.Should().Contain("50.0%");
    }

    [Fact]
    public void AnomalousExposure_OnASmallSample_IsNotReported()
    {
        // 100% of 10 exposures is noise, not evidence of broken bucketing.
        AlertDetector.AnomalousExposure("f", "B", 5_000, 10, 10, Now, Defaults)
            .Should().BeNull();
    }

    [Fact]
    public void Timestamps_AreStampedAtThePrecisionPostgresCanStore()
    {
        // .NET ticks are 100ns; timestamptz stores microseconds. Stamping at full tick precision
        // makes a write response differ from every later read of the same row — nine times out
        // of ten, which is exactly often enough to pass locally and fail in CI.
        var ragged = new DateTimeOffset(2026, 8, 19, 12, 0, 0, TimeSpan.Zero).AddTicks(1_234_567);

        var alert = AlertDetector.KillSwitchChanged("f", true, "ada", ragged);
        alert.Acknowledge("ada", ragged);

        (alert.RaisedAt.Ticks % TimeSpan.TicksPerMicrosecond).Should().Be(0);
        (alert.AcknowledgedAt!.Value.Ticks % TimeSpan.TicksPerMicrosecond).Should().Be(0);

        // Truncated, not rounded up: a timestamp must never claim to be later than the instant
        // it was taken.
        alert.RaisedAt.Should().BeOnOrBefore(ragged);
    }

    [Fact]
    public void AlertsAreAcknowledgedOnce_KeepingTheFirstResponder()
    {
        // Two Admins opening the app at the same moment must not make the record of who
        // responded depend on which request the database happened to serve second.
        var alert = AlertDetector.KillSwitchChanged("f", true, "ada", Now);

        alert.Acknowledge("ada", Now.AddMinutes(1));
        alert.Acknowledge("grace", Now.AddMinutes(2));

        alert.AcknowledgedBy.Should().Be("ada");
        alert.AcknowledgedAt.Should().Be(Now.AddMinutes(1));
    }
}
