using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace OptiPulse.Evaluation.Infrastructure;

/// <summary>
/// Named Polly v8 resilience pipelines for outbound dependencies (Constitution
/// Principle IV / research R4): timeout → retry (jittered backoff) → circuit
/// breaker. "Redis" is consumed by the invalidation subscriber; "Postgres" is
/// available for application-layer write paths (e.g., audit writes) alongside
/// EF Core's own connection-level retry strategy.
/// </summary>
public static class ResilienceExtensions
{
    public const string RedisPipeline = "redis";
    public const string PostgresPipeline = "postgres";

    public static IServiceCollection AddOptiPulseResilience(this IServiceCollection services)
    {
        services.AddResiliencePipeline(RedisPipeline, builder => builder
            .AddTimeout(TimeSpan.FromSeconds(2))
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromMilliseconds(100),
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(15),
            }));

        services.AddResiliencePipeline(PostgresPipeline, builder => builder
            .AddTimeout(TimeSpan.FromSeconds(5))
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromMilliseconds(200),
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(15),
            }));

        return services;
    }
}
