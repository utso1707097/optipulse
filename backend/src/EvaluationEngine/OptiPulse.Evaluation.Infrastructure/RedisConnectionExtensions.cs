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

            var configuration = ConfigurationOptions.Parse(NormalizeRedisUrl(options.ConnectionString));

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

    /// <summary>
    /// Managed providers hand out `redis://` / `rediss://` URLs, which
    /// StackExchange.Redis cannot parse. Converting here means a platform-injected value works
    /// without the operator having to know the difference. Non-URL values pass through.
    /// </summary>
    private static string NormalizeRedisUrl(string connectionString)
    {
        if (!connectionString.StartsWith("redis://", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.StartsWith("rediss://", StringComparison.OrdinalIgnoreCase))
            return connectionString;

        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri))
            return connectionString;

        var userInfo = uri.UserInfo.Split(':', 2);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var port = uri.Port > 0 ? uri.Port : 6379;

        var result = $"{uri.Host}:{port}";
        if (!string.IsNullOrEmpty(password))
            result += $",password={password}";
        if (uri.Scheme.Equals("rediss", StringComparison.OrdinalIgnoreCase))
            result += ",ssl=True";

        return result;
    }
}
