using System.Net;
using System.Text;
using JokerBlazor.Models;
using JokerBlazor.Services;

namespace JokerBlazor.Tests;

public class JokeServiceTests
{
    [Fact]
    public void BuildRequestUrl_without_flags_is_the_canonical_endpoint()
    {
        var url = JokeService.BuildRequestUrl([]);

        Assert.Equal("https://v2.jokeapi.dev/joke/Any", url);
    }

    [Fact]
    public void BuildRequestUrl_keeps_known_flags_in_api_order()
    {
        var url = JokeService.BuildRequestUrl(["explicit", "nsfw", "not-a-flag", "political"]);

        Assert.Equal("https://v2.jokeapi.dev/joke/Any?blacklistFlags=nsfw,political,explicit", url);
    }

    [Fact]
    public void Present_reads_single_and_twopart_jokes()
    {
        var single = JokeText.Present(new Joke
        {
            Type = "single",
            Category = "Programming",
            JokeText = "  A joke.  "
        });
        var twopart = JokeText.Present(new Joke
        {
            Type = "twopart",
            Category = "Programming",
            Setup = "Setup",
            Delivery = "Delivery"
        });

        Assert.NotNull(single);
        Assert.Equal("A joke.", single.Primary);
        Assert.Null(single.Punchline);
        Assert.NotNull(twopart);
        Assert.Equal("Setup", twopart.Primary);
        Assert.Equal("Delivery", twopart.Punchline);
    }

    [Fact]
    public void Present_returns_null_when_the_joke_has_no_text()
    {
        Assert.Null(JokeText.Present(new Joke { Type = "single" }));
        Assert.Null(JokeText.Present(null));
    }

    [Fact]
    public async Task GetJokeAsync_reads_a_single_joke()
    {
        const string json = """
            {
              "error": false,
              "category": "Programming",
              "type": "single",
              "joke": "The glass is twice as big as it needs to be.",
              "id": 23,
              "safe": true,
              "lang": "en"
            }
            """;

        Uri? requested = null;
        var service = new JokeService(new HttpClient(new StubHandler(request =>
        {
            requested = request.RequestUri;
            return Json(json);
        })));

        var result = await service.GetJokeAsync(["nsfw", "explicit"]);

        Assert.True(result.Ok);
        Assert.Equal("https://v2.jokeapi.dev/joke/Any?blacklistFlags=nsfw,explicit", result.RequestUrl);
        Assert.Equal(result.RequestUrl, requested?.AbsoluteUri);
        Assert.Equal("The glass is twice as big as it needs to be.", result.Joke?.Primary);
        Assert.Null(result.Joke?.Punchline);
        Assert.Equal("Programming", result.Joke?.Category);
    }

    [Fact]
    public void ReadResponse_uses_the_api_error_message()
    {
        const string json = """
            {
              "error": true,
              "code": 106,
              "message": "No matching joke found",
              "timestamp": 1795110438583
            }
            """;

        var result = JokeService.ReadResponse("https://v2.jokeapi.dev/joke/Any", false, 400, json);

        Assert.False(result.Ok);
        Assert.Equal("No matching joke found", result.ErrorMessage);
    }

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
