namespace OptiPulse.Flags.Application;

/// <summary>
/// Raises an operational alert about a flag change (FR-025, FR-026).
///
/// <para>A port owned by this context, implemented in the composition root — the same shape as
/// <see cref="IFlagAuditWriter"/>, and for the same reason. Flag Management must not take a
/// dependency on the Audit &amp; Telemetry context: they are separate bounded contexts, and a
/// direct reference would let alerting concerns leak into the aggregate that decides whether a
/// kill switch may be engaged at all.</para>
///
/// <para>Implementations MUST NOT throw. A failure to alert is not a reason to fail the
/// kill-switch operation that caused it — that would make the platform unoperable during
/// exactly the incident the alert is about.</para>
/// </summary>
public interface IOperationalAlerter
{
    Task KillSwitchChangedAsync(
        string flagKey, bool engaged, string actor, CancellationToken cancellationToken = default);
}
