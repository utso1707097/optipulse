namespace OptiPulse.Evaluation.Domain;

/// <summary>
/// Evaluation Engine's own compiled targeting-rule shape. Deliberately NOT the
/// same type as FlagManagement.Domain.TargetingRule — bounded contexts stay
/// decoupled (data-model.md: "each context persists and validates its own
/// aggregates"); the composition root maps between them.
/// </summary>
public readonly record struct CompiledTargetingRule(
    string Attribute,
    CompiledTargetingOperator Operator,
    string[] Values,
    bool Outcome)
{
    public bool Matches(IReadOnlyDictionary<string, string>? attributes)
    {
        if (attributes is null || !attributes.TryGetValue(Attribute, out var value))
            return false;

        return Operator switch
        {
            CompiledTargetingOperator.Equals => Values.Length > 0 && Values[0] == value,
            CompiledTargetingOperator.NotEquals => Values.Length == 0 || Values[0] != value,
            CompiledTargetingOperator.In => Array.IndexOf(Values, value) >= 0,
            CompiledTargetingOperator.Contains => Values.Length > 0 && value.Contains(Values[0], StringComparison.Ordinal),
            CompiledTargetingOperator.GreaterThan => TryCompareNumeric(value, out int cmp) && cmp > 0,
            CompiledTargetingOperator.LessThan => TryCompareNumeric(value, out int cmp2) && cmp2 < 0,
            _ => false,
        };
    }

    private bool TryCompareNumeric(string value, out int comparison)
    {
        comparison = 0;
        if (Values.Length == 0) return false;
        if (!double.TryParse(value, out var actual) || !double.TryParse(Values[0], out var threshold))
            return false;
        comparison = actual.CompareTo(threshold);
        return true;
    }
}

public enum CompiledTargetingOperator
{
    Equals,
    In,
    NotEquals,
    GreaterThan,
    LessThan,
    Contains,
}
