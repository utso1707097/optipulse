namespace OptiPulse.Audit.Application;

/// <summary>
/// Reads aggregated exposure counts (FR-020, SC-008). This MVP slice provides a
/// simple per-flag count sufficient to validate that evaluations reconcile with
/// recorded exposures; full variant-grouped windowed aggregation (VariantExposureCount)
/// is a Phase 5 (US3) concern once Experiments/Variants exist.
/// </summary>
public interface IExposureAggregator
{
    Task<long> GetExposureCountAsync(string flagKey, CancellationToken cancellationToken = default);
}
