namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response;

/// <summary>
/// Курсы валют на конкретную дату
/// </summary>
/// <param name="LastUpdatedAt">Дата обновления данных</param>
/// <param name="Currencies">Список курсов валют</param>
public record CurrenciesOnDate(DateTime LastUpdatedAt, Currency[] Currencies);