namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Config;

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
    /// Дефолтная валюта курс которой нужно считать
    /// </summary>
    public string DefaultCurrency { get; init; }

    /// <summary>
    /// Адрес внешнего апи для запросов
    /// </summary>
    public string BaseUrl { get; init; }
}