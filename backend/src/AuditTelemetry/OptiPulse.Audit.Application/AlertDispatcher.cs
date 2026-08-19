using OptiPulse.Audit.Domain;

namespace OptiPulse.Audit.Application;

/// <summary>
/// What happened to an alert. Returned rather than logged in place, because this project keeps
/// no logging dependency in the Application layer — the caller owns how a degraded delivery is
/// reported.
/// </summary>
public sealed record AlertDispatchResult(Alert? Alert, bool WasDuplicate, Exception? DeliveryError)
{
    public bool DeliveryDegraded => Alert is not null && DeliveryError is not null;
}

/// <summary>
/// Persists an alert, then tries to notify (FR-026).
///
/// <para><b>The order is the guarantee.</b> The spec requires that critical state is never
/// conveyed solely by a possibly-lost push, so the history write happens FIRST and delivery is
/// attempted only after it has committed. A push provider being down, a token being stale or a
/// device sitting in a tunnel then costs the operator promptness and never the information —
/// they open the app and the alert is there.</para>
///
/// <para>Notifier failures are captured rather than thrown for the same reason. If delivery
/// could fail the whole operation, a third-party outage would start failing the kill-switch
/// request that raised the alert — turning a notification problem into an inability to operate
/// the platform during exactly the incident the alert was about.</para>
/// </summary>
public sealed class AlertDispatcher(IAlertStore store, IAlertNotifier notifier)
{
    public async Task<AlertDispatchResult> DispatchAsync(Alert alert, CancellationToken ct = default)
    {
        var stored = await store.RaiseAsync(alert, ct);

        // Null means the dedupe key already existed: the condition is still standing and has
        // already been reported. Notifying again is what dedupe exists to prevent.
        if (stored is null) return new AlertDispatchResult(null, WasDuplicate: true, null);

        try
        {
            var devices = await store.ActiveDevicesAsync(ct);
            if (devices.Count > 0) await notifier.NotifyAsync(stored, devices, ct);
            return new AlertDispatchResult(stored, WasDuplicate: false, null);
        }
        catch (Exception ex)
        {
            return new AlertDispatchResult(stored, WasDuplicate: false, ex);
        }
    }
}
