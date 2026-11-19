using JokerBlazor.Models;

namespace JokerBlazor;

public sealed class FavoriteBook
{
    private readonly List<JokeCard> _items = [];

    public IReadOnlyList<JokeCard> Items => _items;

    public bool Contains(int id) => _items.Any(joke => joke.Id == id);

    public bool Add(JokeCard joke)
    {
        if (joke.Id <= 0 || Contains(joke.Id))
        {
            return false;
        }

        _items.Insert(0, joke);
        return true;
    }

    public bool Remove(int id) => _items.RemoveAll(joke => joke.Id == id) > 0;

    public IReadOnlyList<JokeCard> Search(string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return _items;
        }

        return _items.Where(joke => Matches(joke, term)).ToList();
    }

    public void Replace(IEnumerable<JokeCard>? jokes)
    {
        _items.Clear();
        if (jokes is null)
        {
            return;
        }

        foreach (var joke in jokes)
        {
            if (joke.Id > 0 && !Contains(joke.Id))
            {
                _items.Add(joke);
            }
        }
    }

    private static bool Matches(JokeCard joke, string term) =>
        joke.Primary.Contains(term, StringComparison.OrdinalIgnoreCase)
        || (joke.Punchline?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
        || (joke.Category?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false);
}
