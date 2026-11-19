using System.Text.Json.Serialization;
using JokerBlazor.Models;

namespace JokerBlazor;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(Joke))]
[JsonSerializable(typeof(Flags))]
[JsonSerializable(typeof(JokeApiError))]
public sealed partial class JokeJsonContext : JsonSerializerContext
{
}
