using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OptiPulse.Audit.Application;
using OptiPulse.Audit.Domain;
using OptiPulse.IntegrationTests.Fixtures;
using Xunit;

namespace OptiPulse.IntegrationTests.Alerts;

/// <summary>
/// T065 — the guarantee the whole alerting design rests on: the history is written BEFORE
/// anyone tries to deliver, so a failed push costs promptness and never information.
///
/// This is the spec's own clarification made executable ("critical state is never conveyed
/// solely by a possibly-lost push"). Without a test, "we persist first" is a claim about an
/// ordering that is invisible in normal operation — everything looks identical until the day
/// the notifier is down, which is the day it matters.
/// </summary>
[Collection(OptiPulseTestCollection.Name)]
public sealed class AlertHistoryTests(OptiPulseTestFixture fixture)
{
    private sealed class ExplodingNotifier : IAlertNotifier
    {
        public int Attempts { get; private set; }

        public Task NotifyAsync(Alert alert, IReadOnlyList<PushDevice> devices, CancellationToken ct = default)
        {
            Attempts++;
            throw new HttpRequestException("push provider unavailable");
        }
    }

    [Fact]
    public async Task AnAlertIsPersisted_EvenWhenDeliveryThrows()
    {
        using var scope = fixture.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IAlertStore>();
        var notifier = new ExplodingNotifier();
        var dispatcher = new AlertDispatcher(store, notifier);

        // A registered device, so delivery is actually attempted rather than skipped.
        await store.RegisterDeviceAsync(Guid.CreateVersion7(), DevicePlatform.Ios, $"tok-{Guid.NewGuid():N}");

        var flagKey = $"history-{Guid.NewGuid():N}";
        var alert = AlertDetector.KillSwitchChanged(flagKey, engaged: true, "ada", DateTimeOffset.UtcNow);

        var result = await dispatcher.DispatchAsync(alert);

        result.Alert.Should().NotBeNull("the history write must precede delivery");
        result.DeliveryError.Should().NotBeNull("the notifier really did fail");
        result.DeliveryDegraded.Should().BeTrue();
        notifier.Attempts.Should().Be(1);

        // And it is READABLE afterwards, which is the part an operator depends on.
        var history = await store.ListAsync(unacknowledgedOnly: false, limit: 200);
        history.Should().Contain(a => a.FlagKey == flagKey);
    }

    [Fact]
    public async Task DispatchDoesNotThrow_WhenTheNotifierDoes()
    {
        // A push outage must not fail the kill-switch request that raised the alert. Letting it
        // propagate would make the platform unoperable during exactly the incident the alert is
        // about — a notification problem escalated into an availability problem.
        using var scope = fixture.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IAlertStore>();
        var dispatcher = new AlertDispatcher(store, new ExplodingNotifier());

        await store.RegisterDeviceAsync(Guid.CreateVersion7(), DevicePlatform.Android, $"tok-{Guid.NewGuid():N}");

        var act = async () => await dispatcher.DispatchAsync(
            AlertDetector.KillSwitchChanged($"f-{Guid.NewGuid():N}", true, "ada", DateTimeOffset.UtcNow));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task AStandingCondition_IsRecordedOnce_AndNotifiedOnce()
    {
        // Dedupe is enforced by the unique index, not by a read-then-write, so two detector
        // passes racing each other still produce one alert and one notification.
        using var scope = fixture.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IAlertStore>();
        var notifier = new CountingNotifier();
        var dispatcher = new AlertDispatcher(store, notifier);

        await store.RegisterDeviceAsync(Guid.CreateVersion7(), DevicePlatform.Ios, $"tok-{Guid.NewGuid():N}");

        var flagKey = $"standing-{Guid.NewGuid():N}";
        var thresholds = new AlertThresholds();

        // TIME IS PINNED, and must stay pinned. The dedupe key buckets on absolute time
        // (ticks / windowLength), so two observations a minute apart share a bucket only when
        // they fall inside the same 15-minute window. With DateTimeOffset.UtcNow this test
        // failed whenever it happened to run within a minute of :00, :15, :30 or :45 — about
        // one run in fifteen, and it did exactly that on main at 16:29:42.
        //
        // Straddling the boundary is CORRECT behaviour, not a bug: a condition that is still
        // standing after fifteen minutes should alert again. What was wrong was asserting
        // deduplication while letting the wall clock decide whether deduplication applied.
        var now = new DateTimeOffset(2026, 8, 19, 12, 0, 0, TimeSpan.Zero);

        var first = await dispatcher.DispatchAsync(
            AlertDetector.ErrorRateSpike(flagKey, 120, 1_000, now, thresholds)!);
        // Same 15-minute window as `now` by construction, not by luck.
        var second = await dispatcher.DispatchAsync(
            AlertDetector.ErrorRateSpike(flagKey, 130, 1_000, now.AddMinutes(1), thresholds)!);

        first.WasDuplicate.Should().BeFalse();
        second.WasDuplicate.Should().BeTrue("the same window must not alert twice");
        notifier.Attempts.Should().Be(1, "an operator whose phone buzzes repeatedly stops reading alerts");

        var history = await store.ListAsync(unacknowledgedOnly: false, limit: 200);
        history.Count(a => a.FlagKey == flagKey).Should().Be(1);
    }

    private sealed class CountingNotifier : IAlertNotifier
    {
        public int Attempts { get; private set; }

        public Task NotifyAsync(Alert alert, IReadOnlyList<PushDevice> devices, CancellationToken ct = default)
        {
            Attempts++;
            return Task.CompletedTask;
        }
    }
}
