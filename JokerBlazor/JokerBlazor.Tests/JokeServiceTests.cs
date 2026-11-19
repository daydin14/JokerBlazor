using System.Net;
using System.Text;
using JokerBlazor.Models;
using JokerBlazor.Services;

namespace JokerBlazor.Tests;

public class JokeServiceTests
{
    [Fact]
    public void Safe_default_url_blacklists_the_unsafe_flags()
    {
        var url = JokeRequest.BuildUrl(JokeQuery.SafeDefault());

        Assert.Equal("https://v2.jokeapi.dev/joke/Any?blacklistFlags=nsfw,racist,sexist,explicit", url);
    }

    [Fact]
    public void BuildUrl_adds_category_and_type_and_keeps_flag_order()
    {
        var url = JokeRequest.BuildUrl(new JokeQuery
        {
            Category = "programming",
            JokeType = JokeCatalog.TypeSingle,
            Blacklist = ["explicit", "nsfw", "made-up", "political"]
        });

        Assert.Equal("https://v2.jokeapi.dev/joke/Programming?blacklistFlags=nsfw,political,explicit&type=single", url);
    }

    [Fact]
    public void Both_types_omit_the_type_parameter()
    {
        var url = JokeRequest.BuildUrl(new JokeQuery { JokeType = JokeCatalog.TypeBoth, Blacklist = [] });

        Assert.Equal("https://v2.jokeapi.dev/joke/Any", url);
    }

    [Fact]
    public void Present_reads_single_and_twopart_jokes()
    {
        var single = JokeText.Present(new Joke { Id = 3, Type = "single", Category = "Programming", JokeText = "  A joke.  " });
        var twopart = JokeText.Present(new Joke { Id = 4, Type = "twopart", Category = "Pun", Setup = "Setup", Delivery = "Delivery" });

        Assert.NotNull(single);
        Assert.Equal(3, single.Id);
        Assert.Equal("A joke.", single.Primary);
        Assert.False(single.HasPunchline);
        Assert.NotNull(twopart);
        Assert.Equal("Delivery", twopart.Punchline);
    }

    [Fact]
    public async Task GetJokeAsync_requests_the_filter_url()
    {
        const string json = """
            {"error":false,"category":"Programming","type":"single","joke":"Twice as big.","id":23,"safe":true,"lang":"en"}
            """;
        Uri? requested = null;
        var service = new JokeService(new HttpClient(new StubHandler(request =>
        {
            requested = request.RequestUri;
            return Json(json);
        })));

        var result = await service.GetJokeAsync(new JokeQuery { Blacklist = ["nsfw"] });

        Assert.True(result.Ok);
        Assert.Equal("https://v2.jokeapi.dev/joke/Any?blacklistFlags=nsfw", result.RequestUrl);
        Assert.Equal(result.RequestUrl, requested?.AbsoluteUri);
        Assert.Equal(23, result.Joke?.Id);
    }

    [Fact]
    public void ReadResponse_uses_the_api_error_message()
    {
        const string json = """{"error":true,"code":106,"message":"No matching joke found","timestamp":1}""";

        var result = JokeService.ReadResponse("https://v2.jokeapi.dev/joke/Any", false, 400, json);

        Assert.False(result.Ok);
        Assert.Equal("No matching joke found", result.ErrorMessage);
    }

    [Fact]
    public void History_walks_back_and_forward_and_drops_the_forward_branch()
    {
        var history = new SessionHistory();
        history.Append(Card(1, "one"));
        history.Append(Card(2, "two"));
        Assert.True(history.Back());
        Assert.Equal(1, history.Current?.Id);
        history.Append(Card(3, "three"));

        Assert.False(history.HasSeen(2));
        Assert.False(history.CanForward);
        Assert.Equal(3, history.Current?.Id);
        Assert.True(history.Back());
        Assert.True(history.Forward());
    }

    [Fact]
    public void Favorites_ignore_duplicates_and_search()
    {
        var book = new FavoriteBook();
        Assert.True(book.Add(Card(5, "cargo cult")));
        Assert.False(book.Add(Card(5, "cargo cult")));
        Assert.Single(book.Search("cargo"));
        Assert.Empty(book.Search("missing"));
        Assert.True(book.Remove(5));
        Assert.Empty(book.Items);
    }

    [Fact]
    public void Joke_of_the_day_is_stable_for_a_date()
    {
        var today = new DateOnly(2026, 11, 19);

        Assert.Equal(JokeOfTheDay.IdFor(today), JokeOfTheDay.IdFor(today));
        Assert.NotEqual(JokeOfTheDay.IdFor(today), JokeOfTheDay.IdFor(today.AddDays(1)));
        Assert.Equal(JokeOfTheDay.IdFor(today), JokeOfTheDay.IdFor(today.AddDays(JokeOfTheDay.Range)));
        Assert.Equal(JokeOfTheDay.IdFor(today), JokeOfTheDay.Candidates(today)[0]);
    }

    [Fact]
    public void Quiz_accepts_a_normalized_punchline()
    {
        Assert.True(QuizMatch.IsCorrect("Mitosis!", "mitosis"));
        Assert.False(QuizMatch.IsCorrect("a", "mitosis"));
    }

    private static JokeCard Card(int id, string text) => new(id, "Pun", "single", text, null);

    private static HttpResponseMessage Json(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(respond(request));
    }
}
