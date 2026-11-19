using JokerBlazor.Models;

namespace JokerBlazor;

public sealed class RatingBook
{
    private readonly Dictionary<int, JokeVote> _votes = [];

    public int? ScoreFor(int id) => _votes.TryGetValue(id, out var vote) ? vote.Score : null;

    public void Set(JokeCard joke, int score)
    {
        if (joke.Id <= 0 || score == 0)
        {
            _votes.Remove(joke.Id);
            return;
        }

        _votes[joke.Id] = new JokeVote(score > 0 ? 1 : -1, joke);
    }

    public IReadOnlyList<JokeCard> Top() =>
        _votes.Values.Where(vote => vote.Score > 0).Select(vote => vote.Joke).ToList();

    public IReadOnlyList<JokeVote> All() => _votes.Values.ToList();

    public void Replace(IEnumerable<JokeVote>? votes)
    {
        _votes.Clear();
        if (votes is null)
        {
            return;
        }

        foreach (var vote in votes)
        {
            if (vote.Joke.Id > 0 && vote.Score != 0)
            {
                _votes[vote.Joke.Id] = vote with { Score = vote.Score > 0 ? 1 : -1 };
            }
        }
    }
}
