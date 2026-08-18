namespace OptiPulse.Audit.Application;

/// <summary>
/// Reads aggregated exposure counts (FR-020, SC-008).
/// </summary>
public interface IExposureAggregator
{
    Task<long> GetExposureCountAsync(string flagKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Per-variant exposure counts for a flag — the shape the analytics view needs to compare
    /// arms of an experiment (T055). Counts are of EXPOSURES, i.e. how many times each variant
    /// was actually served, not how many contexts were eligible; those differ whenever a flag
    /// is evaluated more than once per context, and conflating them would inflate the
    /// denominator of any conversion rate computed from it.
    /// </summary>
    Task<IReadOnlyList<VariantExposureCount>> GetVariantExposureCountsAsync(
        string flagKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Per-variant conversion counts — the numerator of a conversion rate (T082). Reported
    /// separately from exposures rather than pre-divided, so the caller can see BOTH numbers: a
    /// rate alone hides whether it came from 3 conversions or 3,000, and those warrant very
    /// different confidence in a result.
    /// </summary>
    Task<IReadOnlyList<VariantConversionCount>> GetVariantConversionCountsAsync(
        string flagKey, CancellationToken cancellationToken = default);
}

/// <param name="VariantKey">Null for flag-level exposures recorded outside an experiment.</param>
public sealed record VariantExposureCount(string? VariantKey, long Exposures);

/// <param name="VariantKey">Null for conversions reported without a variant.</param>
public sealed record VariantConversionCount(string? VariantKey, long Conversions);
