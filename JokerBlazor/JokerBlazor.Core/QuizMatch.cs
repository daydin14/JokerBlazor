namespace JokerBlazor;

public static class QuizMatch
{
    public static bool IsCorrect(string? guess, string? punchline)
    {
        var normalizedGuess = Normalize(guess);
        var normalizedPunchline = Normalize(punchline);
        if (normalizedGuess.Length == 0 || normalizedPunchline.Length == 0)
        {
            return false;
        }

        if (normalizedGuess == normalizedPunchline)
        {
            return true;
        }

        return normalizedGuess.Length >= 4 && normalizedPunchline.Contains(normalizedGuess, StringComparison.Ordinal);
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var letters = value.Where(character => char.IsLetterOrDigit(character) || char.IsWhiteSpace(character)).ToArray();
        var collapsed = string.Join(' ', new string(letters).Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return collapsed.ToLowerInvariant();
    }
}
