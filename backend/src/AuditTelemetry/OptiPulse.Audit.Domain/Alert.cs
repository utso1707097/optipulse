namespace OptiPulse.Audit.Domain;

/// <summary>What kind of condition raised an alert (FR-025, FR-026).</summary>
public enum AlertKind
{
    /// <summary>An Admin engaged or released a kill switch.</summary>
    KillSwitchChanged = 1,

    /// <summary>Evaluation error rate crossed the configured threshold.</summary>
    ErrorRateSpike = 2,

    /// <summary>A variant's exposure share diverged from its configured weight.</summary>
    AnomalousExposure = 3,
}

public enum AlertSeverity
{
    Info = 1,
    Warning = 2,
    Critical = 3,
}

/// <summary>
/// A condition an operator needs to know about, recorded durably before anyone tries to deliver
/// it (FR-026).
///
/// <para><b>The history is the channel; push is an optimisation.</b> The spec is explicit that
/// critical state is never conveyed solely by a possibly-lost push, and that ordering is the
/// whole design: an alert is persisted first, and notification is attempted afterwards. A push
/// that fails — device offline, token expired, FCM down — costs the operator promptness, never
/// the information. Reversing the order would mean an outage in a third-party notification
/// service could silently erase the record that anything happened.</para>
///
/// <para><b>DedupeKey</b> is uniquely indexed. Detectors run on a timer and evaluate a standing
/// condition, so an error-rate spike that lasts ten minutes would otherwise raise an alert on
/// every pass. An operator whose phone buzzes forty times for one incident learns to ignore the
/// alerts, which is a worse failure than not sending them.</para>
/// </summary>
public sealed class Alert
{
    private Alert() { } // EF

    private Alert(
        Guid id,
        DateTimeOffset raisedAt,
        AlertKind kind,
        AlertSeverity severity,
        string title,
        string detail,
        string dedupeKey,
        string? flagKey)
    {
        Id = id;
        RaisedAt = raisedAt;
        Kind = kind;
        Severity = severity;
        Title = title;
        Detail = detail;
        DedupeKey = dedupeKey;
        FlagKey = flagKey;
    }

    public Guid Id { get; private set; }
    public DateTimeOffset RaisedAt { get; private set; }
    public AlertKind Kind { get; private set; }
    public AlertSeverity Severity { get; private set; }

    /// <summary>Null for conditions that are not about one flag.</summary>
    public string? FlagKey { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string Detail { get; private set; } = string.Empty;
    public string DedupeKey { get; private set; } = string.Empty;

    public DateTimeOffset? AcknowledgedAt { get; private set; }
    public string? AcknowledgedBy { get; private set; }

    public bool IsAcknowledged => AcknowledgedAt is not null;

    public static Alert Raise(
        AlertKind kind,
        AlertSeverity severity,
        string title,
        string detail,
        string dedupeKey,
        DateTimeOffset now,
        string? flagKey = null)
        => new(Guid.CreateVersion7(), now, kind, severity, title, detail, dedupeKey, flagKey);

    /// <summary>
    /// Records that an operator has seen this. Idempotent: acknowledging twice keeps the FIRST
    /// acknowledgement, because two Admins opening the app at once should not have the record
    /// of who responded depend on which request the database happened to serve second.
    /// </summary>
    public void Acknowledge(string actor, DateTimeOffset now)
    {
        if (IsAcknowledged) return;
        AcknowledgedAt = now;
        AcknowledgedBy = actor;
    }
}
