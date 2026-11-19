using Bunit;
using JokerBlazor.Models;
using JokerBlazor.Tests.Probes;

namespace JokerBlazor.Tests;

public class LogicProbeTests : BunitContext
{
    [Fact]
    public void Probe_renders_filter_url_day_favorites_and_history()
    {
        var book = new FavoriteBook();
        book.Add(new JokeCard(9, "Programming", "single", "cache invalidation", null));
        var history = new SessionHistory();
        history.Append(new JokeCard(9, "Programming", "single", "cache invalidation", null));
        var day = new DateOnly(2026, 11, 19);

        var cut = Render<LogicProbe>(parameters => parameters
            .Add(probe => probe.Query, new JokeQuery { Category = "Pun", JokeType = JokeCatalog.TypeTwoPart, Blacklist = ["nsfw"] })
            .Add(probe => probe.Day, day)
            .Add(probe => probe.Book, book)
            .Add(probe => probe.Term, "cache")
            .Add(probe => probe.History, history));

        Assert.Equal("https://v2.jokeapi.dev/joke/Pun?blacklistFlags=nsfw&type=twopart", cut.Find(".url").TextContent);
        Assert.Equal(JokeOfTheDay.IdFor(day).ToString(), cut.Find(".day").TextContent);
        Assert.Equal("1", cut.Find(".favorites").TextContent);
        Assert.Equal("9", cut.Find(".history").TextContent);
    }
}
