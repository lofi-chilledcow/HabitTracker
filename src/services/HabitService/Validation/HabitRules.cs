namespace HabitService.Validation;

public static class HabitRules
{
    private static readonly HashSet<string> ValidFrequencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "daily",
        "weekly"
    };

    public static void Validate(string name, string frequency, byte? targetDaysPerWeek)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Habit name is required.", nameof(name));

        if (name.Trim().Length > 200)
            throw new ArgumentException("Habit name must be 200 characters or fewer.", nameof(name));

        if (string.IsNullOrWhiteSpace(frequency) || !ValidFrequencies.Contains(frequency))
            throw new ArgumentException("Frequency must be daily or weekly.", nameof(frequency));

        if (string.Equals(frequency, "weekly", StringComparison.OrdinalIgnoreCase)
            && (targetDaysPerWeek is null or < 1 or > 7))
        {
            throw new ArgumentException("Weekly habits require target days between 1 and 7.", nameof(targetDaysPerWeek));
        }

        if (string.Equals(frequency, "daily", StringComparison.OrdinalIgnoreCase) && targetDaysPerWeek is not null)
            throw new ArgumentException("Daily habits must not include target days per week.", nameof(targetDaysPerWeek));
    }
}
