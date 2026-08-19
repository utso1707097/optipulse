using OptiPulse.Audit.Domain;

namespace OptiPulse.Audit.Application;

/// <summary>
/// Thresholds for the standing conditions. Configurable because "anomalous" is a deployment
/// judgement, not a universal constant.
/// </summary>
public sealed record AlertThresholds
{
    /// <summary>Error share, as a fraction, above which an alert is raised.</summary>
    public double ErrorRateThreshold { get; init; } = 0.05;

    /// <summary>
    /// Below this many evaluations, error rate is not reported at all. Two errors out of three
    /// requests is 67% and means nothing — a threshold on a tiny sample fires constantly during
    /// quiet periods and trains people to ignore it.
    /// </summary>
    public int MinimumSampleForErrorRate { get; init; } = 100;

    /// <summary>
    /// How far a variant's observed exposure share may drift from its configured weight, in
    /// basis points, before it is called anomalous.
    /// </summary>
    public int ExposureDriftToleranceBasisPoints { get; init; } = 1_000;

    /// <summary>As above: sampling noise dominates below this many exposures.</summary>
    public int MinimumSampleForExposure { get; init; } = 500;

    /// <summary>
    /// Width of the dedupe window. A standing condition raises at most one alert per window,
    /// however often the detector runs.
    /// </summary>
    public TimeSpan DedupeWindow { get; init; } = TimeSpan.FromMinutes(15);
}

/// <summary>
/// The rules that turn observations into alerts (FR-025).
///
/// <para>PURE FUNCTIONS, deliberately. Each takes what it observed and returns an alert or null,
/// touching no clock, no database and no configuration it was not handed. That makes every
/// threshold and every dedupe key directly testable, which matters because the failure mode of
/// an alerting system is not usually a crash — it is alerting too much, or not at all, and both
/// are quiet.</para>
/// </summary>
public static class AlertDetector
{
    /// <summary>
    /// A kill switch changed state. Always alertable: it is a deliberate human act with
    /// immediate effect on live traffic, and the other Admins need to know it happened even —
    /// especially — when they were not the one who did it.
    /// </summary>
    public static Alert KillSwitchChanged(
        string flagKey, bool engaged, string actor, DateTimeOffset now) =>
        Alert.Raise(
            kind: AlertKind.KillSwitchChanged,
            // Engaging is Critical; releasing is a Warning. Both are worth knowing, but only
            // one of them means a feature just stopped serving for everybody.
            severity: engaged ? AlertSeverity.Critical : AlertSeverity.Warning,
            title: engaged ? $"Kill switch ENGAGED on {flagKey}" : $"Kill switch released on {flagKey}",
            detail: $"{actor} {(engaged ? "engaged" : "released")} the kill switch for '{flagKey}'. "
                  + (engaged
                      ? "All evaluations now return the flag's default outcome."
                      : "Evaluation has resumed under the flag's configured rules."),
            // Not time-bucketed: every change is a distinct event, and collapsing an
            // engage/release/engage sequence into one alert would hide the flapping.
            dedupeKey: $"killswitch:{flagKey}:{now.UtcTicks}",
            now: now,
            flagKey: flagKey);

    /// <summary>
    /// Evaluation error rate over a window. Returns null when the condition does not hold or the
    /// sample is too small to mean anything.
    /// </summary>
    public static Alert? ErrorRateSpike(
        string flagKey,
        int errorCount,
        int totalCount,
        DateTimeOffset now,
        AlertThresholds thresholds)
    {
        if (totalCount < thresholds.MinimumSampleForErrorRate) return null;

        double rate = errorCount / (double)totalCount;
        if (rate <= thresholds.ErrorRateThreshold) return null;

        return Alert.Raise(
            kind: AlertKind.ErrorRateSpike,
            severity: AlertSeverity.Critical,
            title: $"Evaluation error rate {rate:P1} on {flagKey}",
            detail: $"{errorCount} of {totalCount} evaluations of '{flagKey}' failed "
                  + $"({rate:P1}), above the {thresholds.ErrorRateThreshold:P0} threshold.",
            dedupeKey: DedupeKey("errorrate", flagKey, now, thresholds.DedupeWindow),
            now: now,
            flagKey: flagKey);
    }

    /// <summary>
    /// A variant is being served at a share its configured weight does not explain — the signal
    /// that bucketing or targeting is misbehaving, which corrupts an experiment's result while
    /// leaving every individual request looking fine.
    /// </summary>
    public static Alert? AnomalousExposure(
        string flagKey,
        string variantKey,
        int configuredWeightBasisPoints,
        int observedExposures,
        int totalExposures,
        DateTimeOffset now,
        AlertThresholds thresholds)
    {
        if (totalExposures < thresholds.MinimumSampleForExposure) return null;

        int observedBp = (int)(observedExposures * 10_000L / totalExposures);
        int driftBp = Math.Abs(observedBp - configuredWeightBasisPoints);
        if (driftBp <= thresholds.ExposureDriftToleranceBasisPoints) return null;

        return Alert.Raise(
            kind: AlertKind.AnomalousExposure,
            severity: AlertSeverity.Warning,
            title: $"Variant '{variantKey}' exposure is off target on {flagKey}",
            detail: $"'{variantKey}' is configured at {configuredWeightBasisPoints / 100.0:F1}% "
                  + $"but was served to {observedBp / 100.0:F1}% of {totalExposures} exposures. "
                  + "Experiment results for this flag may not be trustworthy.",
            dedupeKey: DedupeKey($"exposure:{variantKey}", flagKey, now, thresholds.DedupeWindow),
            now: now,
            flagKey: flagKey);
    }

    /// <summary>
    /// Buckets the timestamp so a condition that persists across several detector passes maps to
    /// ONE key per window. Without this, a spike lasting ten minutes with a one-minute detector
    /// produces ten identical alerts, and an operator whose phone buzzes ten times for one
    /// incident learns to ignore the alerts entirely.
    /// </summary>
    private static string DedupeKey(string prefix, string flagKey, DateTimeOffset now, TimeSpan window)
    {
        long windowIndex = now.UtcTicks / window.Ticks;
        return $"{prefix}:{flagKey}:{windowIndex}";
    }
}
