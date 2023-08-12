using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response.ExternalAPI;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Contracts;

public interface ICurrencyService
{
    /// <summary>
    /// Получает текущий курс для валюты
    /// </summary>
    /// <param name="currencyCode">Валюта, для которой необходимо получить курс</param>
    /// <returns>Курсов валюты относительно базовой валюты</returns>
    public Task<ExchangeRates> GetCurrencyRateAsync(string currencyCode);

    /// <summary>
    /// Получает курс для валюты, актуальный на <paramref name="date"/>
    /// </summary>
    /// <param name="currencyCode">Валюта, для которой необходимо получить курс</param>
    /// <param name="date">Дата, на которую нужно получить курс валюты</param>
    /// <returns>Курсов валюты относительно базовой валюты на дату</returns>
    public Task<ExchangeRatesWithDate> GetHistoricalCurrencyRateAsync(string currencyCode, DateOnly date);

    /// <summary>
    /// Получает текущие настройки приложения
    /// </summary>
    /// <returns> Возвращает текущие настройки приложения</returns>
    public Task<SettingsResult> GetSettingsAsync();
}