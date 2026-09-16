using System.Text.Json.Serialization;

namespace SoftwareFactory;

public sealed class Ticket
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("description")]
    public string Description { get; init; } = "";

    [JsonPropertyName("acceptanceCriteria")]
    public string[] AcceptanceCriteria { get; init; } = [];
}
