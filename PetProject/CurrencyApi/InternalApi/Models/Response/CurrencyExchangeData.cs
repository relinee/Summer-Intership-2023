using System.Text.Json.Serialization;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response;

public record CurrencyExchangeData
{
    [JsonPropertyName("meta")]
    public Meta Meta { get; init; }
    
    [JsonPropertyName("data")]
    public Dictionary<string, CurrencyData> Data { get; init; }
}