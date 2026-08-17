using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using OptiPulse.Audit.Application;
using OptiPulse.Audit.Domain;

namespace OptiPulse.Audit.Infrastructure;

/// <summary>
/// Bounded-channel exposure writer (research R6). <see cref="Record"/> is a
/// non-blocking, allocation-light enqueue — the evaluation caller never waits on
/// persistence. If the channel is full (persistence falling behind), the oldest
/// exposure is dropped rather than blocking evaluation, since exposure telemetry
/// tolerates a small loss rate (SC-008 targets 1% reconciliation margin) far
/// better than the hot path tolerates blocking.
/// </summary>
public sealed class ExposureWriter : IExposureRecorder
{
    private readonly Channel<ExposureEvent> _channel;
    private readonly ILogger<ExposureWriter> _logger;

    public ExposureWriter(ILogger<ExposureWriter> logger)
    {
        _logger = logger;
        _channel = Channel.CreateBounded<ExposureEvent>(new BoundedChannelOptions(capacity: 10_000)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false,
        });
    }

    internal ChannelReader<ExposureEvent> Reader => _channel.Reader;

    public void Record(string flagKey, string? variantKey, string? contextKey, long snapshotVersion, Guid? experimentId = null)
    {
        var evt = new ExposureEvent(
            Id: 0, // assigned by the database on persist
            Timestamp: DateTimeOffset.UtcNow,
            FlagKey: flagKey,
            ExperimentId: experimentId,
            VariantKey: variantKey,
            ContextKey: contextKey,
            SnapshotVersion: snapshotVersion);

        if (!_channel.Writer.TryWrite(evt))
        {
            _logger.LogWarning("Exposure channel full — dropping event for flag {FlagKey}", flagKey);
        }
    }
}
