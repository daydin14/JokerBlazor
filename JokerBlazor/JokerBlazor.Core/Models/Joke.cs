using System.Text.Json.Serialization;

namespace JokerBlazor.Models;

public sealed class Joke
{
    [JsonPropertyName("error")]
    public bool Error { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("joke")]
    public string? JokeText { get; set; }

    [JsonPropertyName("setup")]
    public string? Setup { get; set; }

    [JsonPropertyName("delivery")]
    public string? Delivery { get; set; }

    [JsonPropertyName("flags")]
    public Flags? Flags { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("safe")]
    public bool Safe { get; set; }

    [JsonPropertyName("lang")]
    public string? Lang { get; set; }
}
