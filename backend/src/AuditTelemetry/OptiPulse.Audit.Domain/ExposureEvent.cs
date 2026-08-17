namespace OptiPulse.Audit.Domain;

/// <summary>
/// Append-only, high-volume exposure record (data-model.md, FR-020). Written
/// asynchronously off the evaluation hot path (research R6) — never constructed
/// or persisted synchronously during flag evaluation.
/// </summary>
public sealed record ExposureEvent(
    long Id,
    DateTimeOffset Timestamp,
    string FlagKey,
    Guid? ExperimentId,
    string? VariantKey,
    string? ContextKey,
    long SnapshotVersion);
