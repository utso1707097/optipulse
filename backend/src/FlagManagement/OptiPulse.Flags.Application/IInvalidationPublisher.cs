using OptiPulse.Flags.Domain;

namespace OptiPulse.Flags.Application;

/// <summary>
/// Publishes cache-invalidation notices to every evaluation node (research R3).
///
/// Publish happens AFTER the database commit, never before: a message announcing a version
/// that failed to persist would make every node reject the eventual real update as stale
/// (the snapshot ignores versions it has already seen), leaving the fleet permanently behind.
/// </summary>
public interface IInvalidationPublisher
{
    Task PublishFlagChangedAsync(Flag flag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kill-switch changes are published as their own message type so subscribers can apply
    /// kill-switch precedence (SC-002, &lt;100ms) even if they race a stale re-enable.
    /// </summary>
    Task PublishKillSwitchAsync(Flag flag, CancellationToken cancellationToken = default);
}
