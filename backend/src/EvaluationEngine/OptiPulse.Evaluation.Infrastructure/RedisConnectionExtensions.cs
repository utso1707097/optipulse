using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace OptiPulse.Evaluation.Infrastructure;

public static class RedisConnectionExtensions
{
    public static IServiceCollection AddOptiPulseRedis(this IServiceCollection services, IConfiguration configuration)
    {
        // Manual binding (not the reflection-based OptionsBuilder.Bind extension) —
        // this project is held to the AOT/trim-analyzer bar (Principle II), and
        // RedisOptions is small enough that manual construction costs nothing.
        services.Configure<RedisOptions>(options =>
        {
            var section = configuration.GetSection(RedisOptions.SectionName);
            options.ConnectionString = section[nameof(RedisOptions.ConnectionString)]
                ?? throw new InvalidOperationException(
                    $"Configuration section '{RedisOptions.SectionName}:{nameof(RedisOptions.ConnectionString)}' is required.");
            options.InvalidationChannel =
                section[nameof(RedisOptions.InvalidationChannel)] ?? options.InvalidationChannel;
        });

        // Direct RedisOptions projection alongside IOptions<RedisOptions>, so
        // consumers (e.g. InvalidationSubscriber) can take it as a plain
        // constructor dependency without unwrapping IOptions themselves.
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<RedisOptions>>().Value);

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = sp.GetRequiredService<RedisOptions>();

            var configuration = ConfigurationOptions.Parse(options.ConnectionString);

            // Principle IV (fail-safe): an unreachable Redis must NOT stop the API
            // from starting. Redis carries invalidation messages, not the flag data
            // itself — the snapshot serves last-known-good without it, which is
            // exactly the degraded mode the kill-switch design assumes. With the
            // default AbortOnConnectFail=true, Connect() throws and the whole host
            // dies at DI resolution, turning a recoverable dependency outage into
            // total unavailability. False instead lets the multiplexer reconnect in
            // the background; InvalidationSubscriber resubscribes on recovery.
            configuration.AbortOnConnectFail = false;

            return ConnectionMultiplexer.Connect(configuration);
        });

        return services;
    }
}
