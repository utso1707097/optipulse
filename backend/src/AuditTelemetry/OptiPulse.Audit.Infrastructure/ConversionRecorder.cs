using Microsoft.EntityFrameworkCore;
using OptiPulse.Audit.Application;
using OptiPulse.Audit.Domain;
using OptiPulse.SharedKernel;

namespace OptiPulse.Audit.Infrastructure;

public sealed class ConversionRecorder(AuditDbContext dbContext, TimeProvider timeProvider)
    : IConversionRecorder
{
    public async Task<Result<ConversionResult>> RecordAsync(
        string flagKey,
        string goal,
        string idempotencyKey,
        string? contextKey,
        string? variantKey,
        Guid? experimentId,
        decimal? value,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(flagKey))
            return Error.Validation("Conversion.FlagKey.Required", "flagKey is required.");
        if (string.IsNullOrWhiteSpace(goal))
            return Error.Validation("Conversion.Goal.Required", "goal is required.");
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return Error.Validation(
                "Conversion.IdempotencyKey.Required",
                "idempotencyKey is required — without it a retried report would double-count.");

        var conversion = new ConversionEvent(
            Id: 0,
            Timestamp: timeProvider.GetUtcNow(),
            FlagKey: flagKey,
            ExperimentId: experimentId,
            VariantKey: variantKey,
            ContextKey: contextKey,
            Goal: goal,
            IdempotencyKey: idempotencyKey,
            Value: value);

        dbContext.ConversionEvents.Add(conversion);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return new ConversionResult(Recorded: true, Duplicate: false);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // Insert-then-catch rather than check-then-insert: two concurrent retries of the
            // same conversion would both pass a prior existence check and both insert. Letting
            // the unique index arbitrate is the only version that is correct under concurrency.
            dbContext.ChangeTracker.Clear();
            return new ConversionResult(Recorded: false, Duplicate: true);
        }
    }

    /// <summary>Postgres reports a unique-constraint breach as SQLSTATE 23505.</summary>
    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException?.GetType().Name == "PostgresException"
        && ex.InnerException.ToString().Contains("23505", StringComparison.Ordinal);
}
