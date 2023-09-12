using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Contracts;

public interface ICachedCurrencyAPI
{
    /// <summary>
    /// Получает текущий курс
    /// </summary>
    /// <param name="currencyType">Валюта, для которой необходимо получить курс</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Текущий курс</returns>
    public Task<CurrencyDTO> GetCurrentCurrencyAsync(CurrencyType currencyType, CancellationToken cancellationToken);

    /// <summary>
    /// Получает курс валюты, актуальный на <paramref name="date"/>
    /// </summary>
    /// <param name="currencyType">Валюта, для которой необходимо получить курс</param>
    /// <param name="date">Дата, на которую нужно получить курс валют</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Курс на дату</returns>
    public Task<CurrencyDTO> GetCurrencyOnDateAsync(CurrencyType currencyType, DateOnly date, CancellationToken cancellationToken);
}