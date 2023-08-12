namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Config;

/// <summary>
/// Настройки API
/// </summary>
public record ApiSettings
{
    /// <summary>
    /// Ключ API
    /// </summary>
    public string ApiKey { get; init; }
    
    /// <summary>
    /// Базовая валюта относительно которой должны считать курс
    /// </summary>
    public string BaseCurrency { get; init; }
    
    /// <summary>
    /// Дефолтная валюта курс которой нужно считать
    /// </summary>
    public string DefaultCurrency { get; init; }
    
    /// <summary>
    /// Количество знаков после запятой для курса
    /// </summary>
    public int DecimalPlaces { get; init; }
    
    /// <summary>
    /// Адрес внешнего апи для запросов
    /// </summary>
    public string BaseUrl { get; init; }
}