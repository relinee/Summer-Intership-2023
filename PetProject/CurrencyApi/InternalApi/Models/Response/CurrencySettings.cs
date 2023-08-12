namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response;

/// <summary>
/// Настройки для курса валют
/// </summary>
/// <param name="BaseCurrencyCode">Базовая валюта относительно которой считается курс</param>
/// <param name="hasAvailableRequests">Доступность новых запросов ко внешнему API</param>
public record CurrencySettings(CurrencyType BaseCurrencyCode, bool HasAvailableRequests);