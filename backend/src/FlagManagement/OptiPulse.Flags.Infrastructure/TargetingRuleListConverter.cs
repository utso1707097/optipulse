using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OptiPulse.Flags.Domain;

namespace OptiPulse.Flags.Infrastructure;

/// <summary>
/// EF Core value conversion for the TargetingRules value-object list: stored as a
/// JSON column rather than a normalized table (adequate for this MVP's read-mostly
/// bootstrap use; revisit if targeting rules need independent querying). Uses
/// source-generated JSON (not reflection-based JsonSerializer) to stay Native
/// AOT-compatible (constitution Principle III).
/// </summary>
internal static class TargetingRuleListConverter
{
    public static readonly ValueConverter<List<TargetingRule>, string> Instance = new(
        rules => System.Text.Json.JsonSerializer.Serialize(rules, TargetingRuleJsonContext.Default.ListTargetingRule),
        json => System.Text.Json.JsonSerializer.Deserialize(json, TargetingRuleJsonContext.Default.ListTargetingRule)
                ?? new List<TargetingRule>());

    public static readonly ValueComparer<List<TargetingRule>> Comparer = new(
        (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
        list => list.Aggregate(0, (hash, rule) => HashCode.Combine(hash, rule.GetHashCode())),
        list => list.ToList());
}

[JsonSerializable(typeof(List<TargetingRule>))]
internal partial class TargetingRuleJsonContext : JsonSerializerContext;
