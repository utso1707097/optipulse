namespace OptiPulse.Evaluation.Infrastructure;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    /// <summary>Set by <see cref="RedisConnectionExtensions.AddOptiPulseRedis"/>, which
    /// throws at startup if the configuration key is missing — enforced imperatively
    /// there rather than via `required` + reflection-based options validation.</summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>Channel used for flag-change / kill-switch invalidation messages
    /// (contracts/invalidation-channel.md).</summary>
    public string InvalidationChannel { get; set; } = "optipulse:flags:invalidate";
}
