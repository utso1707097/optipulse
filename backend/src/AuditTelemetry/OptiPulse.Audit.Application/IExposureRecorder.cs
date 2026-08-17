namespace OptiPulse.Audit.Application;

/// <summary>
/// Non-blocking exposure recording (FR-020, research R6). Implementations MUST
/// NOT perform I/O on the calling thread — evaluation must never wait on audit
/// persistence. Backed by a bounded in-memory channel, drained asynchronously.
/// </summary>
public interface IExposureRecorder
{
    void Record(string flagKey, string? variantKey, string? contextKey, long snapshotVersion, Guid? experimentId = null);
}
