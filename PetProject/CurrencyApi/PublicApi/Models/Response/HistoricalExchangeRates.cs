using System.Text.Json.Serialization;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Models;

/// <summary>
/// Курс для валюты в определенный день
/// </summary>
public record HistoricalExchangeRates
{
    /// <summary>
    /// Дата актуальности курса
    /// </summary>
    [JsonPropertyName("date")]
    public DateOnly Date { get; init; }
    
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