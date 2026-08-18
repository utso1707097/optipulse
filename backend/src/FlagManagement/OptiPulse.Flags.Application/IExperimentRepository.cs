using OptiPulse.Flags.Domain;
using OptiPulse.SharedKernel;

namespace OptiPulse.Flags.Application;

public interface IExperimentRepository
{
    Task<Experiment?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Experiment>> ListAsync(string? flagKey = null, CancellationToken cancellationToken = default);
    Task AddAsync(Experiment experiment, CancellationToken cancellationToken = default);
    Task<Result> SaveChangesAsync(CancellationToken cancellationToken = default);
}
