using System.Text.Json.Serialization;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response.ExternalAPI;

/// <summary>
/// Последний курс для валюты
/// </summary>
public record LatestExchangeRates
{
    /// <summary>
    /// Код валюты
    /// </summary>
    [JsonPropertyName("code")]
    public string ValueCode { get; init; }
    
    /// <summary>
    /// Текущий курс относительно валюты по умолчанию
    /// </summary>
    [JsonPropertyName("value")]
    public decimal CurrentValueRate { get; init; }
    
}