using OptiPulse.SharedKernel;

namespace OptiPulse.Audit.Application;

public interface IConversionRecorder
{
    /// <summary>
    /// Records a goal completion. Returns <c>false</c> in <c>Recorded</c> when the idempotency
    /// key was already seen — a duplicate is a SUCCESS, not an error: the caller's intent
    /// ("this conversion happened") is already satisfied, and returning a failure would push
    /// host applications toward retry loops that can never succeed.
    /// </summary>
    Task<Result<ConversionResult>> RecordAsync(
        string flagKey,
        string goal,
        string idempotencyKey,
        string? contextKey,
        string? variantKey,
        Guid? experimentId,
        decimal? value,
        CancellationToken cancellationToken = default);
}

public sealed record ConversionResult(bool Recorded, bool Duplicate);
