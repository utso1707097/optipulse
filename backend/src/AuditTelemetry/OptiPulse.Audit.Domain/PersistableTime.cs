namespace OptiPulse.Audit.Domain;

/// <summary>
/// Rounds timestamps to the precision the database can actually store.
/// </summary>
/// <remarks>
/// .NET measures time in 100-nanosecond ticks; PostgreSQL `timestamptz` stores microseconds. An
/// entity stamped at full tick precision therefore returns one value from the write and a
/// different, truncated one from every later read — so an API reports a timestamp the database
/// does not hold, nine times out of ten.
///
/// The way that surfaced is worth remembering: a test comparing a write response against a
/// subsequent read passed whenever the final tick digit happened to be zero. That is often
/// enough to pass locally and fail in CI, which is the same shape as the flaky rollout test —
/// a test that succeeds by luck rather than by the code being right.
///
/// Stamping the truncated value up front makes what is returned and what is stored the same
/// thing by construction, rather than relying on callers to compare loosely.
/// </remarks>
internal static class PersistableTime
{
    public static DateTimeOffset Truncate(DateTimeOffset value) =>
        new(value.Ticks - (value.Ticks % TimeSpan.TicksPerMicrosecond), value.Offset);
}
