namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Config;

/// <summary>
/// Настройки API
/// </summary>
public record ApiSettings
{
    /// <summary>
    /// Дефолтная валюта курс которой нужно считать
    /// </summary>
    public string DefaultCurrency { get; init; }
}