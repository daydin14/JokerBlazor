using System.Text.Json.Serialization;
using JokerBlazor.Models;

namespace JokerBlazor;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(Joke))]
[JsonSerializable(typeof(Flags))]
[JsonSerializable(typeof(JokeApiError))]
[JsonSerializable(typeof(JokeQuery))]
[JsonSerializable(typeof(JokeCard))]
[JsonSerializable(typeof(JokeVote))]
[JsonSerializable(typeof(List<JokeCard>))]
[JsonSerializable(typeof(List<JokeVote>))]
public sealed partial class JokeJsonContext : JsonSerializerContext
{
}
