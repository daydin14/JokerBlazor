using JokerBlazor.Models;

namespace JokerBlazor;

public static class JokeText
{
    public static JokeCard? Present(Joke? joke)
    {
        if (joke is null)
        {
            return null;
        }

        var jokeText = joke.JokeText?.Trim();
        var setup = joke.Setup?.Trim();
        var delivery = joke.Delivery?.Trim();
        var isSingle = string.Equals(joke.Type, "single", StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrWhiteSpace(jokeText) && string.IsNullOrWhiteSpace(setup));

        if (isSingle && !string.IsNullOrWhiteSpace(jokeText))
        {
            return new JokeCard(joke.Id, joke.Category, "single", jokeText, null);
        }

        if (!string.IsNullOrWhiteSpace(setup))
        {
            return new JokeCard(joke.Id, joke.Category, joke.Type ?? "twopart", setup, string.IsNullOrWhiteSpace(delivery) ? null : delivery);
        }

        if (!string.IsNullOrWhiteSpace(delivery))
        {
            return new JokeCard(joke.Id, joke.Category, joke.Type, delivery, null);
        }

        if (!string.IsNullOrWhiteSpace(jokeText))
        {
            return new JokeCard(joke.Id, joke.Category, joke.Type ?? "single", jokeText, null);
        }

        return null;
    }

    public static string ShareText(JokeCard joke, bool includePunchline)
    {
        if (includePunchline && !string.IsNullOrWhiteSpace(joke.Punchline))
        {
            return joke.Primary + "\n\n" + joke.Punchline;
        }

        return joke.Primary;
    }
}
