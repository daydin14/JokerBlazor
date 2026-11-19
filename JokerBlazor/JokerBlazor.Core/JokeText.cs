using JokerBlazor.Models;

namespace JokerBlazor;

public static class JokeText
{
    public static JokePresentation? Present(Joke? joke)
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
            return new JokePresentation(joke.Category, jokeText, null);
        }

        if (!string.IsNullOrWhiteSpace(setup))
        {
            return new JokePresentation(joke.Category, setup, string.IsNullOrWhiteSpace(delivery) ? null : delivery);
        }

        if (!string.IsNullOrWhiteSpace(delivery))
        {
            return new JokePresentation(joke.Category, delivery, null);
        }

        if (!string.IsNullOrWhiteSpace(jokeText))
        {
            return new JokePresentation(joke.Category, jokeText, null);
        }

        return null;
    }
}
