using System.Text.Json.Serialization;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Response;

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
    /// Есть ли еще доступные запросы
    /// </summary>
    [JsonPropertyName("newRequestsAvailable")]
    public bool NewRequestsAvailable { get; init; }

    /// <summary>
    /// Количество знаков после запятой, до которого следует округлять значение курса валют
    /// </summary>
    [JsonPropertyName("currencyRoundCount")]
    public int CurrencyRoundCount { get; init; }
}