using OptiPulse.Audit.Domain;

namespace OptiPulse.Audit.Application;

/// <summary>
/// Best-effort delivery of an already-persisted alert (FR-026).
///
/// <para>Every implementation is allowed to fail, and callers are required not to care. The
/// alert is in the history before this is invoked, so a failed push costs promptness and never
/// information — which is the property the spec asks for when it says critical state is never
/// conveyed solely by a possibly-lost push.</para>
///
/// <para>This is an interface rather than a direct FCM call so that push is a deployment
/// choice, not an architectural dependency. A deployment with no push provider configured is a
/// fully working alerting system that an operator reads in-app; adding FCM later changes one
/// registration and nothing else.</para>
/// </summary>
public interface IAlertNotifier
{
    Task NotifyAsync(Alert alert, IReadOnlyList<PushDevice> devices, CancellationToken ct = default);
}
