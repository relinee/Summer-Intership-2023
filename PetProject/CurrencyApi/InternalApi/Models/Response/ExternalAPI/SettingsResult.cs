using System.Text.Json.Serialization;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response.ExternalAPI;

/// <summary>
/// Настройки приложения
/// </summary>
public record SettingsResult
{
    /// <summary>
    /// Текущий курс валют по умолчанию из конфигурации
    /// </summary>
    [JsonPropertyName("defaultCurrency")]
    public string DefaultCurrency { get; init; }
    
    /// <summary>
    /// Базовая валюта, относительно которой считается курс
    /// </summary>
    [JsonPropertyName("baseCurrency")]
    public string BaseCurrency { get; init; }
    
    /// <summary>
    /// Общее количество доступных запросов, полученное от внешнего API (quotas->month->total)
    /// </summary>
    [JsonPropertyName("requestLimit")]
    public int RequestLimit { get; init; }
    
    /// <summary>
    /// Количество использованных запросов, полученное от внешнего API (quotas->month->used)
    /// </summary>
    [JsonPropertyName("requestCount")]
    public int RequestCount { get; init; }
    
    /// <summary>
    /// Количество знаков после запятой, до которого следует округлять значение курса валют
    /// </summary>
    [JsonPropertyName("currencyRoundCount")]
    public int CurrencyRoundCount { get; init; }
}