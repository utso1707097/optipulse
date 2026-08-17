namespace OptiPulse.Flags.Domain;

/// <summary>Value object: a condition over context attributes selecting an outcome
/// when matched (data-model.md). Ordered within a Flag; first match wins.</summary>
public sealed record TargetingRule(
    string Attribute,
    TargetingOperator Operator,
    IReadOnlyList<string> Values,
    bool Outcome);

public enum TargetingOperator
{
    Equals,
    In,
    NotEquals,
    GreaterThan,
    LessThan,
    Contains,
}
