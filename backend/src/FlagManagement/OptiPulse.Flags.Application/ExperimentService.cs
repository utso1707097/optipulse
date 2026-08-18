using OptiPulse.Flags.Domain;
using OptiPulse.SharedKernel;

namespace OptiPulse.Flags.Application;

/// <summary>
/// Write-side use cases for experiments (T047/T050). Same ordering discipline as
/// <see cref="FlagManagementService"/>: apply the rule, commit, audit, then publish.
/// </summary>
public sealed class ExperimentService(
    IExperimentRepository repository,
    IFlagRepository flagRepository,
    IFlagAuditWriter auditWriter,
    TimeProvider timeProvider)
{
    public Task<IReadOnlyList<Experiment>> ListAsync(string? flagKey, CancellationToken ct = default) =>
        repository.ListAsync(flagKey, ct);

    public Task<Experiment?> GetAsync(Guid id, CancellationToken ct = default) =>
        repository.GetAsync(id, ct);

    public async Task<Result<Experiment>> CreateAsync(
        ActorReference actor,
        string flagKey,
        string name,
        IReadOnlyList<Variant> variants,
        string? conversionGoal,
        CancellationToken ct = default)
    {
        // The experiment references its flag by ID (data-model: cross-aggregate links are by ID),
        // but the flag must actually exist — an experiment on a non-existent flag could never be
        // evaluated and would sit there looking healthy.
        var flag = await flagRepository.GetByKeyAsync(flagKey, ct);
        if (flag is null)
            return Error.NotFound("Flag.NotFound", $"Flag '{flagKey}' was not found.");

        var created = Experiment.Create(flag.Id, flag.Key, name, variants, conversionGoal, timeProvider.GetUtcNow());
        if (created.IsFailure)
            return created.Error;

        var experiment = created.Value;
        await repository.AddAsync(experiment, ct);

        var saved = await repository.SaveChangesAsync(ct);
        if (saved.IsFailure)
            return saved.Error;

        await auditWriter.RecordAsync(
            actor, FlagAuditAction.ExperimentChanged, experiment.Id, null, Describe(experiment), ct);

        return experiment;
    }

    public async Task<Result<Experiment>> UpdateVariantsAsync(
        ActorReference actor,
        Guid id,
        long expectedVersion,
        IReadOnlyList<Variant> variants,
        CancellationToken ct = default)
    {
        var experiment = await repository.GetAsync(id, ct);
        if (experiment is null)
            return Error.NotFound("Experiment.NotFound", $"Experiment '{id}' was not found.");

        if (experiment.Version != expectedVersion)
            return Error.Conflict(
                "Experiment.VersionMismatch",
                $"Experiment is at version {experiment.Version}, not {expectedVersion}. Re-read and retry.");

        var before = Describe(experiment);

        var updated = experiment.UpdateVariants(variants, timeProvider.GetUtcNow());
        if (updated.IsFailure)
            return updated.Error;

        var saved = await repository.SaveChangesAsync(ct);
        if (saved.IsFailure)
            return saved.Error;

        await auditWriter.RecordAsync(
            actor, FlagAuditAction.ExperimentChanged, experiment.Id, before, Describe(experiment), ct);

        return experiment;
    }

    public async Task<Result<Experiment>> ChangeStatusAsync(
        ActorReference actor, Guid id, ExperimentStatus target, CancellationToken ct = default)
    {
        var experiment = await repository.GetAsync(id, ct);
        if (experiment is null)
            return Error.NotFound("Experiment.NotFound", $"Experiment '{id}' was not found.");

        var before = Describe(experiment);
        var now = timeProvider.GetUtcNow();

        var transition = target switch
        {
            ExperimentStatus.Running => experiment.Start(now),
            ExperimentStatus.Concluded => experiment.Conclude(now),
            _ => Result.Failure(Error.Validation(
                "Experiment.Status.Unsupported", "Only Running and Concluded are reachable transitions.")),
        };

        if (transition.IsFailure)
            return transition.Error;

        var saved = await repository.SaveChangesAsync(ct);
        if (saved.IsFailure)
            return saved.Error;

        await auditWriter.RecordAsync(
            actor, FlagAuditAction.ExperimentChanged, experiment.Id, before, Describe(experiment), ct);

        return experiment;
    }

    private static string Describe(Experiment e) =>
        $$"""{"id":"{{e.Id}}","flagKey":"{{e.FlagKey}}","name":"{{e.Name}}","status":"{{e.Status}}","variants":{{e.Variants.Count}},"version":{{e.Version}}}""";
}
