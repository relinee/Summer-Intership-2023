namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Response;

/// <summary>
/// Избранный курс валют
/// </summary>
/// <param name="Name">Название</param>
/// <param name="Currency">Валюта</param>
/// <param name="BaseCurrency">Базовая валюта</param>
public record CurrencyFavouriteModel(string Name, CurrencyType Currency, CurrencyType BaseCurrency);