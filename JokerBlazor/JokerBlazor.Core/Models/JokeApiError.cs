using System.Text.Json.Serialization;

namespace JokerBlazor.Models;

public sealed class JokeApiError
{
    [JsonPropertyName("error")]
    public bool Error { get; set; }

    [JsonPropertyName("internalError")]
    public bool InternalError { get; set; }

    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("causedBy")]
    public string[]? CausedBy { get; set; }

    [JsonPropertyName("additionalInfo")]
    public string? AdditionalInfo { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}
