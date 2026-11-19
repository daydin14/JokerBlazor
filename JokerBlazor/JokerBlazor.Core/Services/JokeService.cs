using System.Net.Http.Headers;
using System.Text.Json;
using JokerBlazor.Models;

namespace JokerBlazor.Services;

public sealed class JokeService(HttpClient http)
{
    public async Task<JokeFetchResult> GetJokeAsync(JokeQuery? query, CancellationToken cancellationToken = default)
    {
        var url = JokeRequest.BuildUrl(query);
        return await SendAsync(url, cancellationToken);
    }

    public async Task<JokeFetchResult> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return JokeFetchResult.Failure(JokeRequest.Host, "That joke id is not valid.");
        }

        return await SendAsync(JokeRequest.BuildByIdUrl(id), cancellationToken);
    }

    public async Task<JokeFetchResult> GetJokeOfTheDayAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        JokeFetchResult? last = null;
        foreach (var id in JokeOfTheDay.Candidates(date))
        {
            cancellationToken.ThrowIfCancellationRequested();
            last = await GetByIdAsync(id, cancellationToken);
            if (last.Ok)
            {
                return last;
            }
        }

        return last ?? JokeFetchResult.Failure(JokeRequest.Host, "Joke of the day is unavailable.");
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
            var card = JokeText.Present(joke);
            if (card is null)
            {
                return JokeFetchResult.Failure(url, "JokeAPI returned a joke with no text.");
            }

            return JokeFetchResult.Success(url, card);
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

    private async Task<JokeFetchResult> SendAsync(string url, CancellationToken cancellationToken)
    {
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

    private static string? FirstMessage(JokeApiError? error)
    {
        if (!string.IsNullOrWhiteSpace(error?.Message))
        {
            return error.Message;
        }

        return string.IsNullOrWhiteSpace(error?.AdditionalInfo) ? null : error.AdditionalInfo;
    }
}

public sealed record JokeFetchResult(bool Ok, string RequestUrl, JokeCard? Joke, string? ErrorMessage)
{
    public static JokeFetchResult Success(string url, JokeCard joke) => new(true, url, joke, null);

    public static JokeFetchResult Failure(string url, string message) => new(false, url, null, message);
}
