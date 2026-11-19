using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Blazored.LocalStorage;
using JokerBlazor.Models;

namespace JokerBlazor.Services;

public sealed class JokeLibrary(ILocalStorageService storage)
{
    public const string FavoritesKey = "joker-favorites";
    public const string RatingsKey = "joker-ratings";
    public const string SettingsKey = "joker-settings";
    public const string OfflineKey = "joker-offline-jokes";
    public const int OfflineLimit = 8;

    private bool _loaded;

    public FavoriteBook Favorites { get; } = new();

    public RatingBook Ratings { get; } = new();

    public JokeQuery Settings { get; private set; } = JokeQuery.SafeDefault();

    public async Task EnsureLoadedAsync()
    {
        if (_loaded)
        {
            return;
        }

        Favorites.Replace(await ReadAsync<List<JokeCard>>(FavoritesKey, JokeJsonContext.Default.ListJokeCard));
        Ratings.Replace(await ReadAsync<List<JokeVote>>(RatingsKey, JokeJsonContext.Default.ListJokeVote));
        Settings = await ReadAsync<JokeQuery>(SettingsKey, JokeJsonContext.Default.JokeQuery) ?? JokeQuery.SafeDefault();
        Settings.Category = JokeRequest.NormalizeCategory(Settings.Category);
        Settings.Blacklist = JokeRequest.NormalizeFlags(Settings.Blacklist).ToList();
        if (Settings.JokeType is not (JokeCatalog.TypeSingle or JokeCatalog.TypeTwoPart or JokeCatalog.TypeBoth))
        {
            Settings.JokeType = JokeCatalog.TypeBoth;
        }

        _loaded = true;
    }

    public async Task SaveSettingsAsync(JokeQuery query)
    {
        Settings = query;
        await WriteAsync(SettingsKey, query, JokeJsonContext.Default.JokeQuery);
    }

    public async Task<bool> ToggleFavoriteAsync(JokeCard joke)
    {
        var added = !Favorites.Contains(joke.Id);
        if (added)
        {
            Favorites.Add(joke);
        }
        else
        {
            Favorites.Remove(joke.Id);
        }

        await WriteAsync(FavoritesKey, Favorites.Items.ToList(), JokeJsonContext.Default.ListJokeCard);
        return added;
    }

    public async Task RemoveFavoriteAsync(int id)
    {
        Favorites.Remove(id);
        await WriteAsync(FavoritesKey, Favorites.Items.ToList(), JokeJsonContext.Default.ListJokeCard);
    }

    public async Task RateAsync(JokeCard joke, int score)
    {
        var current = Ratings.ScoreFor(joke.Id);
        Ratings.Set(joke, current == score ? 0 : score);
        await WriteAsync(RatingsKey, Ratings.All().ToList(), JokeJsonContext.Default.ListJokeVote);
    }

    public async Task RememberOfflineAsync(JokeCard joke)
    {
        var cached = await ReadAsync<List<JokeCard>>(OfflineKey, JokeJsonContext.Default.ListJokeCard) ?? [];
        cached.RemoveAll(item => item.Id == joke.Id);
        cached.Insert(0, joke);
        if (cached.Count > OfflineLimit)
        {
            cached.RemoveRange(OfflineLimit, cached.Count - OfflineLimit);
        }

        await WriteAsync(OfflineKey, cached, JokeJsonContext.Default.ListJokeCard);
    }

    public async Task<IReadOnlyList<JokeCard>> OfflineJokesAsync() =>
        await ReadAsync<List<JokeCard>>(OfflineKey, JokeJsonContext.Default.ListJokeCard) ?? [];

    private async Task<T?> ReadAsync<T>(string key, JsonTypeInfo<T> type) where T : class
    {
        var json = await storage.GetItemAsStringAsync(key);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize(json, type);
    }

    private ValueTask WriteAsync<T>(string key, T value, JsonTypeInfo<T> type) =>
        storage.SetItemAsStringAsync(key, JsonSerializer.Serialize(value, type));
}
