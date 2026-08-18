using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OptiPulse.Audit.Domain;
using OptiPulse.Resilience;
using Polly.Registry;

namespace OptiPulse.Audit.Infrastructure;

/// <summary>
/// Drains queued exposure events to persistent storage off the evaluation hot
/// path (research R6). Batches writes to keep DB round-trips low under load.
/// </summary>
public sealed class ExposureDrainService(
    ExposureWriter writer,
    IServiceScopeFactory scopeFactory,
    ResiliencePipelineProvider<string> pipelineProvider,
    ILogger<ExposureDrainService> logger) : BackgroundService
{
    private const int BatchSize = 200;
    private static readonly TimeSpan BatchWindow = TimeSpan.FromSeconds(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batch = new List<ExposureEvent>(BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            batch.Clear();
            using var windowCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            windowCts.CancelAfter(BatchWindow);

            try
            {
                while (batch.Count < BatchSize &&
                       await writer.Reader.WaitToReadAsync(windowCts.Token))
                {
                    while (batch.Count < BatchSize && writer.Reader.TryRead(out var evt))
                    {
                        batch.Add(evt);
                    }
                }
            }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
            {
                // Batch window elapsed — flush whatever we have.
            }

            if (batch.Count == 0)
                continue;

            try
            {
                using var scope = scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
                await dbContext.ExposureEvents.AddRangeAsync(batch, stoppingToken);

                // Principle IV: the batch flush is an outbound persistence call, so it carries
                // the "postgres" pipeline (timeout + jittered retry + circuit breaker). Safe to
                // retry here precisely because this runs OFF the hot path — evaluation already
                // returned, so a retry costs latency no caller is waiting on. A dropped batch
                // would show up as an SC-008 reconciliation gap.
                var pipeline = pipelineProvider.GetPipeline(ResilienceExtensions.PostgresPipeline);
                await pipeline.ExecuteAsync(
                    async token => await dbContext.SaveChangesAsync(token), stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Exposure persistence failure never propagates to evaluation —
                // it already returned. Log and continue draining.
                logger.LogError(ex, "Failed to persist a batch of {Count} exposure events", batch.Count);
            }
        }
    }
}
