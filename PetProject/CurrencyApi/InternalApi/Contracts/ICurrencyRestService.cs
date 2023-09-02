using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Contracts;

public interface ICurrencyRestService
{
    /// <summary>
    /// Получить текущий курс валюты
    /// </summary>
    /// <param name="currencyType">Валюта получения курса</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Курс в формате CurrencyDTO</returns>
    public Task<CurrencyDTO> GetCurrentCurrencyRateAsync(CurrencyType currencyType, CancellationToken cancellationToken);

    /// <summary>
    /// Получить курс валюты на дату
    /// </summary>
    /// <param name="currencyType">Валюта получения курса</param>
    /// <param name="date">Дата получения курса</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Курс в формате CurrencyDTO</returns>
    public Task<CurrencyDTO> GetCurrencyRateOnDateAsync(CurrencyType currencyType, DateOnly date,
        CancellationToken cancellationToken);

    /// <summary>
    /// Получить текущие настройки
    /// </summary>
    /// <returns>Текущие настройки</returns>
    public Task<CurrencySettings> GetSettingsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Создать задачу для пересчета кеша на новую базовую валюту
    /// </summary>
    /// <param name="newBaseCurrency">Новая базовая валюта</param>
    /// <returns>Guid созданной задачи</returns>
    public Task<Guid> CreateTaskToRecalculateCacheToNewBaseCurrencyAsync(CurrencyType newBaseCurrency, CancellationToken cancellationToken);
}