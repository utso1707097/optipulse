using Microsoft.EntityFrameworkCore;
using OptiPulse.Audit.Application;
using OptiPulse.Audit.Domain;

namespace OptiPulse.Audit.Infrastructure;

public sealed class AlertStore(AuditDbContext dbContext, TimeProvider timeProvider) : IAlertStore
{
    public async Task<Alert?> RaiseAsync(Alert alert, CancellationToken ct = default)
    {
        dbContext.Alerts.Add(alert);

        try
        {
            await dbContext.SaveChangesAsync(ct);
            return alert;
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // Insert-then-catch, matching ConversionRecorder and for the same reason: two
            // detector passes evaluating the same standing condition would both pass a prior
            // existence check and both insert. Letting the unique index arbitrate is the only
            // version that is correct under concurrency.
            dbContext.ChangeTracker.Clear();
            return null;
        }
    }

    public async Task<IReadOnlyList<Alert>> ListAsync(
        bool unacknowledgedOnly, int limit, CancellationToken ct = default)
    {
        var query = dbContext.Alerts.AsNoTracking();
        if (unacknowledgedOnly) query = query.Where(a => a.AcknowledgedAt == null);

        return await query
            .OrderByDescending(a => a.RaisedAt)
            .Take(Math.Clamp(limit, 1, 200))
            .ToListAsync(ct);
    }

    public async Task<Alert?> AcknowledgeAsync(Guid alertId, string actor, CancellationToken ct = default)
    {
        var alert = await dbContext.Alerts.FirstOrDefaultAsync(a => a.Id == alertId, ct);
        if (alert is null) return null;

        alert.Acknowledge(actor, timeProvider.GetUtcNow());
        await dbContext.SaveChangesAsync(ct);
        return alert;
    }

    public async Task<PushDevice> RegisterDeviceAsync(
        Guid userId, DevicePlatform platform, string token, CancellationToken ct = default)
    {
        var existing = await dbContext.PushDevices.FirstOrDefaultAsync(d => d.Token == token, ct);
        var now = timeProvider.GetUtcNow();

        if (existing is not null)
        {
            // Re-registration is a touch, not a new row. An app that registers on every launch
            // would otherwise accumulate a row per launch and send one alert several times to
            // the same phone.
            existing.Touch(now);
            await dbContext.SaveChangesAsync(ct);
            return existing;
        }

        var device = PushDevice.Register(userId, platform, token, now);
        dbContext.PushDevices.Add(device);
        await dbContext.SaveChangesAsync(ct);
        return device;
    }

    public async Task<IReadOnlyList<PushDevice>> ActiveDevicesAsync(CancellationToken ct = default) =>
        await dbContext.PushDevices.AsNoTracking().Where(d => d.RevokedAt == null).ToListAsync(ct);

    public async Task RevokeDeviceAsync(string token, CancellationToken ct = default)
    {
        var device = await dbContext.PushDevices.FirstOrDefaultAsync(d => d.Token == token, ct);
        if (device is null) return;

        device.Revoke(timeProvider.GetUtcNow());
        await dbContext.SaveChangesAsync(ct);
    }

    /// <summary>Postgres reports a unique-constraint breach as SQLSTATE 23505.</summary>
    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException?.GetType().Name == "PostgresException"
        && ex.InnerException.ToString().Contains("23505", StringComparison.Ordinal);
}
