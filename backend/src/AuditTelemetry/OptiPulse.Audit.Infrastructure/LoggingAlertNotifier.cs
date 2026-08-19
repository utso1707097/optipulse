using Microsoft.Extensions.Logging;
using OptiPulse.Audit.Application;
using OptiPulse.Audit.Domain;

namespace OptiPulse.Audit.Infrastructure;

/// <summary>
/// The default notifier: records the delivery attempt and does nothing else.
///
/// <para>This is NOT a stub standing in for missing work. Push is a delivery optimisation over a
/// durable history that is already the source of truth, so a deployment with no push provider is
/// a complete, working alerting system that operators read in-app. Shipping this as the default
/// means the platform has no dependency on a Firebase project, a Google service account, or an
/// Apple Developer membership — none of which a self-hosted deployment should be forced into to
/// get alerting.</para>
///
/// <para>An FCM/APNs implementation registers in place of this one and must wrap its HTTP calls
/// in a Polly resilience pipeline (constitution Principle II), like every other outbound
/// dependency. No such pipeline is registered today, deliberately: an unused pipeline is
/// enforcement that reports success while providing none, and the anti-pattern gate is there to
/// catch exactly that.</para>
/// </summary>
public sealed class LoggingAlertNotifier(ILogger<LoggingAlertNotifier> logger) : IAlertNotifier
{
    public Task NotifyAsync(
        Alert alert, IReadOnlyList<PushDevice> devices, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Alert {AlertId} ({Kind}, {Severity}) would notify {DeviceCount} device(s): {Title}. "
            + "No push provider is configured; the alert is readable in the in-app history.",
            alert.Id, alert.Kind, alert.Severity, devices.Count, alert.Title);

        return Task.CompletedTask;
    }
}
