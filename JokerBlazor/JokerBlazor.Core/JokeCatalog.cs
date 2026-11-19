namespace JokerBlazor;

public static class JokeCatalog
{
    public const string DefaultCategory = "Any";
    public const string TypeBoth = "both";
    public const string TypeSingle = "single";
    public const string TypeTwoPart = "twopart";

    public static readonly string[] Categories = ["Any", "Programming", "Pun", "Misc", "Dark", "Spooky", "Christmas"];

    public static readonly string[] BlacklistFlags = ["nsfw", "religious", "political", "racist", "sexist", "explicit"];

    public static readonly string[] SafeDefaultFlags = ["nsfw", "racist", "sexist", "explicit"];
}
