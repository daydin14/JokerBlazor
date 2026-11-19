using JokerBlazor.Models;

namespace JokerBlazor;

public sealed class SessionHistory
{
    private readonly List<JokeCard> _items = [];
    private int _index = -1;

    public JokeCard? Current => _index >= 0 && _index < _items.Count ? _items[_index] : null;

    public bool CanBack => _index > 0;

    public bool CanForward => _index >= 0 && _index < _items.Count - 1;

    public IReadOnlyList<JokeCard> Items => _items;

    public bool HasSeen(int id) => id > 0 && _items.Any(joke => joke.Id == id);

    public void Append(JokeCard joke)
    {
        if (_index < _items.Count - 1)
        {
            _items.RemoveRange(_index + 1, _items.Count - (_index + 1));
        }

        _items.Add(joke);
        _index = _items.Count - 1;
    }

    public bool Back()
    {
        if (!CanBack)
        {
            return false;
        }

        _index--;
        return true;
    }

    public bool Forward()
    {
        if (!CanForward)
        {
            return false;
        }

        _index++;
        return true;
    }

    public bool MoveTo(int id)
    {
        var index = _items.FindIndex(joke => joke.Id == id);
        if (index < 0)
        {
            return false;
        }

        _index = index;
        return true;
    }
}
