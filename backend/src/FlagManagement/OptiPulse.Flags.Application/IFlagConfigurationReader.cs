using OptiPulse.Flags.Domain;

namespace OptiPulse.Flags.Application;

/// <summary>
/// Read port used by the composition root (Api host) to bootstrap the Evaluation
/// Engine's in-memory snapshot from persisted flag configuration (R2). Deliberately
/// narrow — write/authoring use cases (create, edit, kill-switch) are Phase 5 (US3)
/// and are not part of this MVP slice.
/// </summary>
public interface IFlagConfigurationReader
{
    Task<IReadOnlyList<Flag>> GetActiveFlagsAsync(CancellationToken cancellationToken = default);
}
