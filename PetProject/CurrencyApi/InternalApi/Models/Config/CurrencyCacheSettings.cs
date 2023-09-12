namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Config;

/// <summary>
/// Настроки для кеширования курсов валют
/// </summary>
public record CurrencyCacheSettings
{
    /// <summary>
    /// Путь сохранения кеша
    /// </summary>
    public string Directory { init; get; }
    
    /// <summary>
    /// Время актуальности кеша в часах
    /// </summary>
    public int ExpirationTimeInHours { init; get; }
}