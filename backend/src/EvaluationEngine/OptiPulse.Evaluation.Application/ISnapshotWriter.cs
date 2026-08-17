using OptiPulse.Evaluation.Domain;

namespace OptiPulse.Evaluation.Application;

/// <summary>Write side of the snapshot store — used by composition-root bootstrap
/// and the Redis invalidation subscriber, never by the evaluation hot path itself.</summary>
public interface ISnapshotWriter
{
    void LoadInitial(FlagSnapshot snapshot);
    void ApplyDelta(CompiledFlag updated, long newVersion);
}
