namespace OptiPulse.Evaluation.Domain;

/// <summary>
/// Request-scoped evaluation input (data-model.md). Readonly struct — no heap
/// allocation on the evaluation hot path (Principle II). ContextKey is optional:
/// a missing key resolves to the flag's default and is treated as anonymous
/// (spec edge case).
/// </summary>
public readonly record struct EvaluationContext(
    string FlagKey,
    string? ContextKey,
    IReadOnlyDictionary<string, string>? Attributes);
