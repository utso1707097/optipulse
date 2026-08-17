namespace OptiPulse.SharedKernel;

/// <summary>Guard clauses for validating inputs at boundaries. Throws on violation —
/// use at construction time for invariants, not as a substitute for Result-based
/// validation of user input (see Result.cs).</summary>
public static class Guard
{
    public static string AgainstNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"'{paramName}' must not be null or whitespace.", paramName);
        return value;
    }

    public static T AgainstNull<T>(T? value, string paramName) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(paramName);
        return value;
    }

    public static int AgainstOutOfRange(int value, int min, int max, string paramName)
    {
        if (value < min || value > max)
            throw new ArgumentOutOfRangeException(paramName, value, $"Must be between {min} and {max}.");
        return value;
    }
}
