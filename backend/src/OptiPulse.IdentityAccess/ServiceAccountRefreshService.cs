using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OptiPulse.IdentityAccess;

/// <summary>
/// Keeps the service-account snapshot current. Bounds how long a revoked SDK key keeps working
/// (see the trade documented on <see cref="ServiceAccountAuthenticator"/>).
///
/// Failure here must not stop the host: the snapshot simply stays on its last-known-good
/// contents, which matches the fail-safe posture of the rest of the system (Principle IV).
/// </summary>
public sealed class ServiceAccountRefreshService(
    IServiceAccountAuthenticator authenticator,
    ILogger<ServiceAccountRefreshService> logger) : BackgroundService
{
    public static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(60);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await authenticator.RefreshAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(
                    ex, "Service-account snapshot refresh failed; keeping last-known-good set");
            }

            try
            {
                await Task.Delay(RefreshInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
