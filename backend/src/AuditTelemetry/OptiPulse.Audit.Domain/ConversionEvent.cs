namespace OptiPulse.Audit.Domain;

/// <summary>
/// A goal completion reported by the host application — the OUTCOME half of an experiment
/// (FR-021, T082).
///
/// Without this, telemetry answers only "who saw which variant". A conversion rate is
/// conversions ÷ exposures, so with no conversions there is no numerator and no experiment can
/// ever be decided — the platform could run an A/B test but never read its result.
///
/// <para><b>IdempotencyKey</b> is supplied by the caller and uniquely indexed. Host applications
/// retry: a network timeout after the server already committed is indistinguishable, from the
/// client's side, from one that never arrived. Without a dedupe key the retry double-counts a
/// purchase, and inflating one arm's numerator is precisely how an experiment silently reports
/// the wrong winner.</para>
/// </summary>
public sealed record ConversionEvent(
    long Id,
    DateTimeOffset Timestamp,
    string FlagKey,
    Guid? ExperimentId,
    string? VariantKey,
    string? ContextKey,
    string Goal,
    string IdempotencyKey,
    decimal? Value);
