namespace JokerBlazor.Models;

public sealed class JokeQuery
{
    public string Category { get; set; } = global::JokerBlazor.JokeCatalog.DefaultCategory;

    public string JokeType { get; set; } = global::JokerBlazor.JokeCatalog.TypeBoth;

    public List<string> Blacklist { get; set; } = [.. global::JokerBlazor.JokeCatalog.SafeDefaultFlags];

    public static JokeQuery SafeDefault() => new();
}
