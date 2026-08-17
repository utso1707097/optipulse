using System.Text.Json.Serialization;

namespace OptiPulse.Evaluation.Infrastructure;

/// <summary>Wire schema for the Redis Pub/Sub invalidation channel
/// (contracts/invalidation-channel.md).</summary>
public sealed record InvalidationMessage(
    string Type,
    string FlagKey,
    Guid FlagId,
    long NewVersion,
    bool KillSwitchEngaged,
    DateTimeOffset PublishedAt);

// Source-generated JSON (not reflection-based JsonSerializer) — this project is
// held to the AOT/trim analyzer bar (Principle II).
[JsonSerializable(typeof(InvalidationMessage))]
internal partial class InvalidationMessageJsonContext : JsonSerializerContext;
