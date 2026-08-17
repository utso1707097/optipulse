using OptiPulse.Evaluation.Domain;

namespace OptiPulse.Evaluation.Application;

/// <summary>
/// Bridges the Evaluation Engine to Flag Management's authoritative store WITHOUT
/// Evaluation.Infrastructure depending on FlagManagement directly (bounded context
/// isolation, data-model.md). Implemented as a composition-root adapter in
/// OptiPulse.Api, which is the only layer allowed to know about both contexts.
/// </summary>
public interface IFlagConfigurationProvider
{
    Task<IReadOnlyList<CompiledFlag>> GetAllCompiledFlagsAsync(CancellationToken cancellationToken = default);
    Task<CompiledFlag?> GetCompiledFlagAsync(string flagKey, CancellationToken cancellationToken = default);
}
