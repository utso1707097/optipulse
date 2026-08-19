using OptiPulse.Audit.Application;
using OptiPulse.Flags.Application;

namespace OptiPulse.Api.Adapters;

/// <summary>
/// Adapts Flag Management's alerting port onto the Audit &amp; Telemetry context, keeping the
/// two bounded contexts from referencing each other directly.
/// </summary>
public sealed class OperationalAlerter(
    AlertDispatcher dispatcher,
    TimeProvider timeProvider,
    ILogger<OperationalAlerter> logger) : IOperationalAlerter
{
    public async Task KillSwitchChangedAsync(
        string flagKey, bool engaged, string actor, CancellationToken cancellationToken = default)
    {
        try
        {
            var alert = AlertDetector.KillSwitchChanged(
                flagKey, engaged, actor, timeProvider.GetUtcNow());

            var result = await dispatcher.DispatchAsync(alert, cancellationToken);

            if (result.DeliveryDegraded)
            {
                // Warning, not Error: the alert IS recorded and readable in the app, so this is
                // degraded delivery rather than lost information.
                logger.LogWarning(
                    result.DeliveryError,
                    "Alert {AlertId} for '{FlagKey}' was recorded but push delivery failed.",
                    result.Alert!.Id, flagKey);
            }
        }
        catch (Exception ex)
        {
            // The port's contract is that it does not throw. A kill switch must still engage
            // when alerting is broken — refusing to operate the platform because the notifier
            // is down inverts which of the two matters during an incident.
            logger.LogError(
                ex,
                "Failed to raise a kill-switch alert for '{FlagKey}'. The change itself succeeded.",
                flagKey);
        }
    }
}
