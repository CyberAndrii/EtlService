using System.Text.Json.Serialization;

namespace EtlService.Output;

public record OutputPayer(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("payment")] decimal Payment,
    [property: JsonPropertyName("date")] string Date,
    [property: JsonPropertyName("account_number")] long AccountNumber
);
