using JokerBlazor.Models;

namespace JokerBlazor;

public static class JokeRequest
{
    public const string Host = "https://v2.jokeapi.dev/joke/";

    public static string BuildUrl(JokeQuery? query)
    {
        query ??= JokeQuery.SafeDefault();
        var category = NormalizeCategory(query.Category);
        var flags = NormalizeFlags(query.Blacklist);
        var parts = new List<string>();
        if (flags.Count > 0)
        {
            parts.Add("blacklistFlags=" + string.Join(",", flags));
        }

        if (query.JokeType is JokeCatalog.TypeSingle or JokeCatalog.TypeTwoPart)
        {
            parts.Add("type=" + query.JokeType);
        }

        var url = Host + category;
        return parts.Count == 0 ? url : url + "?" + string.Join("&", parts);
    }

    public static string BuildByIdUrl(int id) => $"{Host}Any?idRange={id}";

    public static string NormalizeCategory(string? category)
    {
        var match = JokeCatalog.Categories.FirstOrDefault(item =>
            string.Equals(item, category, StringComparison.OrdinalIgnoreCase));
        return match ?? JokeCatalog.DefaultCategory;
    }

    public static IReadOnlyList<string> NormalizeFlags(IEnumerable<string>? flags)
    {
        var selected = new HashSet<string>(flags ?? [], StringComparer.OrdinalIgnoreCase);
        return JokeCatalog.BlacklistFlags.Where(selected.Contains).ToArray();
    }
}
