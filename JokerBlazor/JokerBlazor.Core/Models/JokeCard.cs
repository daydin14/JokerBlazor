namespace JokerBlazor.Models;

public sealed record JokeCard(int Id, string? Category, string? Type, string Primary, string? Punchline)
{
    public bool HasPunchline => !string.IsNullOrWhiteSpace(Punchline);
}
