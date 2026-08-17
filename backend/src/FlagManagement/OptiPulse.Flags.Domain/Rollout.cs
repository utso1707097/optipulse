namespace OptiPulse.Flags.Domain;

/// <summary>Value object: percentage rollout configuration (data-model.md).
/// PercentageBasisPoints is 0–10000 for basis-point precision (R1).</summary>
public sealed record Rollout(int PercentageBasisPoints, string Salt)
{
    public static Rollout FromPercentage(double percentage, string salt) =>
        new((int)Math.Round(percentage * 100), salt);
}
