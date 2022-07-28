using System.Text.Json.Serialization;

namespace EtlService.Output;

public record OutputRoot(
    [property: JsonPropertyName("city")] string City,
    [property: JsonIgnore] Dictionary<string, OutputService> ServicesByName
)
{
    [JsonPropertyName("services")]
    public IReadOnlyList<OutputService> Services
    {
        get => ServicesByName.Values.ToArray();
    }

    [JsonPropertyName("total")]
    public int Total => Services.Count;
}
