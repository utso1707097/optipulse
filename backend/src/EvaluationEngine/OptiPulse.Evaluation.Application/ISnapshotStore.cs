using OptiPulse.Evaluation.Domain;

namespace OptiPulse.Evaluation.Application;

/// <summary>Read access to the current in-memory flag snapshot (research R2).
/// Implemented in Infrastructure as a lock-free, atomically-swapped reference.</summary>
public interface ISnapshotStore
{
    FlagSnapshot Current { get; }
}
