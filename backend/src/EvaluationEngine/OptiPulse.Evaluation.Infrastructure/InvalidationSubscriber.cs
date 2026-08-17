using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OptiPulse.Evaluation.Application;
using StackExchange.Redis;

namespace OptiPulse.Evaluation.Infrastructure;

/// <summary>
/// Subscribes to the Redis Pub/Sub invalidation channel and applies deltas to the
/// in-memory snapshot (contracts/invalidation-channel.md). Idempotent under
/// at-most-once delivery (ISnapshotWriter.ApplyDelta ignores stale/duplicate
/// versions). A missed message simply leaves the snapshot on its last-known-good
/// state — the periodic reconciliation backstop (research R3) is a Phase 5+
/// hardening concern, not required for this MVP's correctness.
/// </summary>
public sealed class InvalidationSubscriber(
    IConnectionMultiplexer redis,
    ISnapshotWriter snapshotWriter,
    IServiceScopeFactory scopeFactory,
    RedisOptions redisOptions,
    ILogger<InvalidationSubscriber> logger) : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = RedisChannel.Literal(redisOptions.InvalidationChannel);

        // Principle IV (fail-safe): Redis may be unreachable at startup. Since the
        // multiplexer is created with AbortOnConnectFail=false, SubscribeAsync
        // throws rather than blocking — and an unhandled exception out of
        // ExecuteAsync stops the entire host by default
        // (BackgroundServiceExceptionBehavior.StopHost). Retry instead, so the API
        // keeps serving last-known-good evaluations and picks invalidation back up
        // when Redis returns. Once subscribed, StackExchange.Redis restores the
        // subscription itself across later reconnects, so this loop exits.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await redis.GetSubscriber().SubscribeAsync(channel, async (_, message) =>
                {
                    await HandleMessageAsync(message, stoppingToken);
                });

                logger.LogInformation(
                    "Subscribed to invalidation channel {Channel}", redisOptions.InvalidationChannel);
                break;
            }
            catch (RedisException ex)
            {
                logger.LogWarning(
                    ex,
                    "Could not subscribe to invalidation channel {Channel}; serving last-known-good "
                        + "and retrying in {RetrySeconds}s",
                    redisOptions.InvalidationChannel,
                    RetryDelay.TotalSeconds);

                await Task.Delay(RetryDelay, stoppingToken);
            }
        }

        // Keep the background service alive until shutdown; the subscription
        // callback above does the actual work as messages arrive.
        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken).ContinueWith(
            _ => { }, TaskScheduler.Default);
    }

    private async Task HandleMessageAsync(RedisValue rawMessage, CancellationToken cancellationToken)
    {
        InvalidationMessage? message;
        try
        {
            message = System.Text.Json.JsonSerializer.Deserialize(
                (string)rawMessage!, InvalidationMessageJsonContext.Default.InvalidationMessage);
        }
        catch (System.Text.Json.JsonException ex)
        {
            logger.LogWarning(ex, "Discarding malformed invalidation message");
            return;
        }

        if (message is null)
            return;

        // IFlagConfigurationProvider is scoped (it ultimately depends on a
        // scoped DbContext); this subscriber is a singleton, so a fresh scope
        // is created per message rather than capturing the scoped service in
        // the singleton's constructor (a captive-dependency DI violation).
        using var scope = scopeFactory.CreateScope();
        var flagProvider = scope.ServiceProvider.GetRequiredService<IFlagConfigurationProvider>();

        // Subscriber rule 1 (invalidation-channel.md): ignore stale/out-of-order versions.
        var compiled = await flagProvider.GetCompiledFlagAsync(message.FlagKey, cancellationToken);
        if (compiled is null)
        {
            logger.LogWarning("Invalidation message for unknown flag {FlagKey}", message.FlagKey);
            return;
        }

        // Subscriber rule 3: kill-switch precedence — a killed flag must never
        // evaluate enabled even if this message races with a stale re-enable.
        var effective = message.Type == "KillSwitch" && message.KillSwitchEngaged
            ? WithKillSwitch(compiled, engaged: true)
            : compiled;

        snapshotWriter.ApplyDelta(effective, message.NewVersion);
    }

    private static Domain.CompiledFlag WithKillSwitch(Domain.CompiledFlag flag, bool engaged) => new()
    {
        FlagKey = flag.FlagKey,
        DefaultOutcome = flag.DefaultOutcome,
        KillSwitchEngaged = engaged,
        Version = flag.Version,
        TargetingRules = flag.TargetingRules,
        RolloutBasisPoints = flag.RolloutBasisPoints,
        RolloutSalt = flag.RolloutSalt,
    };
}
