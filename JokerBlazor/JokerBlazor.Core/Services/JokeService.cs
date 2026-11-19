using System.Net.Http.Headers;
using System.Text.Json;
using JokerBlazor.Models;

namespace JokerBlazor.Services;

public sealed class JokeService(HttpClient http)
{
    public const string Endpoint = "https://v2.jokeapi.dev/joke/Any";

    public static readonly string[] BlacklistFlags = ["nsfw", "religious", "political", "racist", "sexist", "explicit"];

    public static string BuildRequestUrl(IEnumerable<string>? selectedFlags)
    {
        var selected = new HashSet<string>(selectedFlags ?? [], StringComparer.OrdinalIgnoreCase);
        var flags = BlacklistFlags.Where(selected.Contains).ToArray();
        if (flags.Length == 0)
        {
            return Endpoint;
        }

        return $"{Endpoint}?blacklistFlags={string.Join(",", flags)}";
    }

    public async Task<JokeFetchResult> GetJokeAsync(IEnumerable<string>? selectedFlags, CancellationToken cancellationToken = default)
    {
        var url = BuildRequestUrl(selectedFlags);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            using var response = await http.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return ReadResponse(url, response.IsSuccessStatusCode, (int)response.StatusCode, body);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return JokeFetchResult.Failure(url, "The browser could not reach JokeAPI. Check the connection and try again.");
        }
    }

    public static JokeFetchResult ReadResponse(string url, bool isSuccess, int statusCode, string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (root.TryGetProperty("error", out var errorElement) && errorElement.ValueKind == JsonValueKind.True)
            {
                var apiError = JsonSerializer.Deserialize(body, JokeJsonContext.Default.JokeApiError);
                var message = FirstMessage(apiError) ?? $"JokeAPI returned {statusCode}.";
                return JokeFetchResult.Failure(url, message);
            }

            if (!isSuccess)
            {
                return JokeFetchResult.Failure(url, $"JokeAPI returned {statusCode}.");
            }

            var joke = JsonSerializer.Deserialize(body, JokeJsonContext.Default.Joke);
            var presentation = JokeText.Present(joke);
            if (presentation is null)
            {
                return JokeFetchResult.Failure(url, "JokeAPI returned a joke with no text.");
            }

            return JokeFetchResult.Success(url, presentation);
        }
        catch (JsonException)
        {
            if (!isSuccess)
            {
                return JokeFetchResult.Failure(url, $"JokeAPI returned {statusCode}.");
            }

            return JokeFetchResult.Failure(url, "JokeAPI returned a response that could not be read.");
        }
    }

    private static string? FirstMessage(JokeApiError? error)
    {
        if (!string.IsNullOrWhiteSpace(error?.Message))
        {
            return error.Message;
        }

        if (!string.IsNullOrWhiteSpace(error?.AdditionalInfo))
        {
            return error.AdditionalInfo;
        }

        return null;
    }
}

public sealed record JokeFetchResult(bool Ok, string RequestUrl, JokePresentation? Joke, string? ErrorMessage)
{
    public static JokeFetchResult Success(string url, JokePresentation joke) => new(true, url, joke, null);

    public static JokeFetchResult Failure(string url, string message) => new(false, url, null, message);
}
