using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace OptiPulse.Resilience;

/// <summary>
/// Named Polly v8 resilience pipelines for outbound dependencies (Constitution
/// Principle IV / research R4): timeout → retry (jittered backoff) → circuit breaker.
///
/// Lives in its own cross-cutting project rather than inside a bounded context: several
/// contexts need these pipelines, and having (say) Audit depend on EvaluationEngine to get
/// them would couple two contexts that must stay independent (Principle I). It is deliberately
/// NOT in SharedKernel — that is referenced by Domain projects, including the AOT hot path,
/// which must not acquire a DI or Polly dependency.
///
/// Constitution v2.2.0 forbids decorative pipelines: every pipeline registered here MUST have
/// at least one consumer, and `scripts/check-antipatterns.sh` fails the build otherwise. The
/// evaluation hot path is exempt from resilience wrapping by design (Principle II) — it reads
/// only in-memory snapshot state, so there is no outbound call to wrap.
///
/// Current consumers:
///   redis    — InvalidationSubscriber's channel subscription
///   postgres — AuditLog writes and the ExposureDrainService batch drain
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
