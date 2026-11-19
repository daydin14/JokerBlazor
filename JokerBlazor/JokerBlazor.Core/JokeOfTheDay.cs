namespace JokerBlazor;

public static class JokeOfTheDay
{
    private static readonly DateOnly Epoch = new(2020, 1, 1);
    public const int Range = 300;

    public static int IdFor(DateOnly date)
    {
        var days = date.DayNumber - Epoch.DayNumber;
        if (days < 0)
        {
            days = -days;
        }

        return (days % Range) + 1;
    }

    public static IReadOnlyList<int> Candidates(DateOnly date, int attempts = 6)
    {
        var start = IdFor(date);
        var count = Math.Clamp(attempts, 1, Range);
        return Enumerable.Range(0, count)
            .Select(offset => ((start - 1 + offset) % Range) + 1)
            .ToArray();
    }
}
