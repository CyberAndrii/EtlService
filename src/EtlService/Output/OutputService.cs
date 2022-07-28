using System.Text.Json.Serialization;

namespace EtlService.Output;

public record OutputService(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("payers")] List<OutputPayer> Payers
)
{
    [JsonPropertyName("total")]
    public int Total => Payers.Count;
}
