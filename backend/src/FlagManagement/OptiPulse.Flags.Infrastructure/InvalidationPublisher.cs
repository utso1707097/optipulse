using System.Text.Json;
using System.Text.Json.Serialization;
using OptiPulse.Flags.Application;
using OptiPulse.Flags.Domain;
using StackExchange.Redis;

namespace OptiPulse.Flags.Infrastructure;

/// <summary>
/// Redis Pub/Sub implementation of <see cref="IInvalidationPublisher"/>
/// (contracts/invalidation-channel.md).
///
/// A publish failure does NOT fail the caller's write. The database is the source of truth and
/// the commit already succeeded; nodes that miss the notice keep serving their last-known-good
/// snapshot and converge on the next message or reload (Principle IV). Throwing here would
/// report failure for an operation that actually succeeded, which is the more damaging lie.
/// </summary>
public sealed class InvalidationPublisher(
    IConnectionMultiplexer redis,
    FlagInvalidationOptions options,
    TimeProvider timeProvider) : IInvalidationPublisher
{
    public Task PublishFlagChangedAsync(Flag flag, CancellationToken cancellationToken = default) =>
        PublishAsync("FlagChanged", flag);

    public Task PublishKillSwitchAsync(Flag flag, CancellationToken cancellationToken = default) =>
        PublishAsync("KillSwitch", flag);

    private async Task PublishAsync(string type, Flag flag)
    {
        var message = new FlagInvalidationMessage(
            type, flag.Key, flag.Id, flag.Version, flag.KillSwitchEngaged, timeProvider.GetUtcNow());

        var payload = JsonSerializer.Serialize(
            message, FlagInvalidationJsonContext.Default.FlagInvalidationMessage);

        await redis.GetSubscriber().PublishAsync(
            RedisChannel.Literal(options.InvalidationChannel), payload);
    }
}

/// <summary>Channel name for publishing. Mirrors the evaluation side's RedisOptions; kept
/// separate so Flag Management does not take a dependency on the Evaluation context.</summary>
public sealed class FlagInvalidationOptions
{
    public string InvalidationChannel { get; set; } = "optipulse:flags:invalidate";
}

/// <summary>
/// Wire schema — MUST stay structurally identical to the evaluation side's InvalidationMessage.
/// It is duplicated rather than shared because the two live in different bounded contexts and
/// referencing across them to save a record would couple them (Principle I). The channel
/// contract is documented in contracts/invalidation-channel.md; the round-trip is covered by
/// an integration test so a drift between the two shapes fails the build rather than silently
/// breaking invalidation.
/// </summary>
public sealed record FlagInvalidationMessage(
    string Type,
    string FlagKey,
    Guid FlagId,
    long NewVersion,
    bool KillSwitchEngaged,
    DateTimeOffset PublishedAt);

[JsonSerializable(typeof(FlagInvalidationMessage))]
internal partial class FlagInvalidationJsonContext : JsonSerializerContext;
