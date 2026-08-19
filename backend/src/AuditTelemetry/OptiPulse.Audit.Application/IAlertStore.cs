using OptiPulse.Audit.Domain;

namespace OptiPulse.Audit.Application;

/// <summary>
/// Durable alert history and device registry (FR-026).
///
/// Persisting is separated from notifying on purpose — see <see cref="IAlertNotifier"/>. This
/// interface is the part that must not fail silently; delivery is best-effort on top of it.
/// </summary>
public interface IAlertStore
{
    /// <summary>
    /// Records the alert, or returns the EXISTING one when <paramref name="alert"/>'s dedupe key
    /// has already been used. Returns null when it was a duplicate, so callers can tell whether
    /// there is anything new to notify about.
    /// </summary>
    Task<Alert?> RaiseAsync(Alert alert, CancellationToken ct = default);

    Task<IReadOnlyList<Alert>> ListAsync(bool unacknowledgedOnly, int limit, CancellationToken ct = default);

    Task<Alert?> AcknowledgeAsync(Guid alertId, string actor, CancellationToken ct = default);

    Task<PushDevice> RegisterDeviceAsync(
        Guid userId, DevicePlatform platform, string token, CancellationToken ct = default);

    Task<IReadOnlyList<PushDevice>> ActiveDevicesAsync(CancellationToken ct = default);

    Task RevokeDeviceAsync(string token, CancellationToken ct = default);
}
