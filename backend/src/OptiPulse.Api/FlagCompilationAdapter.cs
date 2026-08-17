using OptiPulse.Evaluation.Application;
using OptiPulse.Evaluation.Domain;
using OptiPulse.Flags.Application;
using OptiPulse.Flags.Domain;

namespace OptiPulse.Api;

/// <summary>
/// Composition-root adapter bridging Flag Management to the Evaluation Engine
/// (Evaluation.Application.IFlagConfigurationProvider). This is the only place
/// in the solution allowed to reference both Flags.Domain and Evaluation.Domain
/// types — everywhere else the two bounded contexts stay decoupled.
/// </summary>
public sealed class FlagCompilationAdapter(IFlagConfigurationReader reader) : IFlagConfigurationProvider
{
    public async Task<IReadOnlyList<CompiledFlag>> GetAllCompiledFlagsAsync(CancellationToken cancellationToken = default)
    {
        var flags = await reader.GetActiveFlagsAsync(cancellationToken);
        return flags.Select(Compile).ToList();
    }

    public async Task<CompiledFlag?> GetCompiledFlagAsync(string flagKey, CancellationToken cancellationToken = default)
    {
        var flags = await reader.GetActiveFlagsAsync(cancellationToken);
        var flag = flags.FirstOrDefault(f => f.Key == flagKey);
        return flag is null ? null : Compile(flag);
    }

    private static CompiledFlag Compile(Flag flag) => new()
    {
        FlagKey = flag.Key,
        DefaultOutcome = flag.DefaultOutcome,
        KillSwitchEngaged = flag.KillSwitchEngaged,
        Version = flag.Version,
        TargetingRules = [.. flag.TargetingRules.Select(CompileRule)],
        RolloutBasisPoints = flag.Rollout?.PercentageBasisPoints,
        RolloutSalt = flag.Rollout?.Salt,
    };

    private static CompiledTargetingRule CompileRule(TargetingRule rule) => new(
        rule.Attribute,
        MapOperator(rule.Operator),
        [.. rule.Values],
        rule.Outcome);

    private static CompiledTargetingOperator MapOperator(TargetingOperator op) => op switch
    {
        TargetingOperator.Equals => CompiledTargetingOperator.Equals,
        TargetingOperator.In => CompiledTargetingOperator.In,
        TargetingOperator.NotEquals => CompiledTargetingOperator.NotEquals,
        TargetingOperator.GreaterThan => CompiledTargetingOperator.GreaterThan,
        TargetingOperator.LessThan => CompiledTargetingOperator.LessThan,
        TargetingOperator.Contains => CompiledTargetingOperator.Contains,
        _ => throw new ArgumentOutOfRangeException(nameof(op), op, "Unknown targeting operator"),
    };
}
