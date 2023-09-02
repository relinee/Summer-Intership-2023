using Fuse8_ByteMinds.SummerSchool.InternalApi.Contracts;
using Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts;
using Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts.Entities;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Exceptions;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Config;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services;

internal class CachedCurrencyService : ICachedCurrencyAPI
{
    
    private readonly ICurrencyAPI _currencyApi;
    private readonly IOptionsMonitor<CurrencyCacheSettings> _cacheSettings;
    private readonly CurrencyRateDbContext _currencyRateDbContext;

    public CachedCurrencyService(
        ICurrencyAPI currencyApi,
        IOptionsMonitor<CurrencyCacheSettings> cacheSettings,
        CurrencyRateDbContext currencyRateDbContext)
    {
        _currencyApi = currencyApi;
        _cacheSettings = cacheSettings;
        _currencyRateDbContext = currencyRateDbContext;
    }
    
    public async Task<CurrencyDTO> GetCurrentCurrencyAsync(CurrencyType currencyType, CancellationToken cancellationToken)
    {
        var cachedCurrencies = await GetDataFromDbWithoutExpirationTime(
            dateTime: DateTimeOffset.UtcNow,
            expirationTimeInHours: _cacheSettings.CurrentValue.ExpirationTimeInHours,
            cancellationToken);
        if (cachedCurrencies != null) return GetCurrentCurrencyDto(cachedCurrencies, currencyType);
        
        var baseCurrency = GetBaseCurrencyFromDb();
        var currentCurrencies = await _currencyApi.GetAllCurrentCurrenciesAsync(baseCurrency, cancellationToken);
        
        CacheCurrenciesOnDateAsync(
            currencies: currentCurrencies,
            date: DateTimeOffset.UtcNow);

        return GetCurrentCurrencyDto(currentCurrencies, currencyType);
    }
    
    public async Task<CurrencyDTO> GetCurrencyOnDateAsync(CurrencyType currencyType, DateOnly date, CancellationToken cancellationToken)
    {
        var cachedCurrencies = await GetDataFromDb(date.ToDateTime(TimeOnly.MaxValue), cancellationToken);
        if (cachedCurrencies != null) return GetCurrentCurrencyDto(cachedCurrencies, currencyType);
        
        var baseCurrency = GetBaseCurrencyFromDb();
        var currenciesRateOnDate = await _currencyApi.GetAllCurrenciesOnDateAsync(baseCurrency, date, cancellationToken);
        
        CacheCurrenciesOnDateAsync(
            currencies: currenciesRateOnDate.Currencies,
            date: currenciesRateOnDate.LastUpdatedAt.ToUniversalTime());

        return GetCurrentCurrencyDto(currenciesRateOnDate.Currencies, currencyType);
    }
    
    /// <summary>
    /// Выполняет поиск курса нужной валюты среди всех курсов и возвращает их 
    /// </summary>
    /// <param name="currencies">Массив курсов валют</param>
    /// <param name="currencyType">Нужный тип валюты</param>
    /// <returns>Тип и курс валюты в формате CurrencyDTO</returns>
    private static CurrencyDTO GetCurrentCurrencyDto(Currency[] currencies, CurrencyType currencyType)
    {
        var currentCurrencyValue = currencies.First(c => c.Code == currencyType.ToString().ToUpperInvariant()).Value;
        return new CurrencyDTO(currencyType, currentCurrencyValue);
    }

    /// <summary>
    /// Получение из базы данных курсов валют без истечения срока давности
    /// </summary>
    /// <param name="dateTime">Дата, на которую нужен курс валют</param>
    /// <param name="expirationTimeInHours">Срок актуальности кеша в часах</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Если срок давности не вышел и данные в базе данных есть, то массив курсов валют. Иначе null</returns>
    private async Task<Currency[]?> GetDataFromDbWithoutExpirationTime(
        DateTimeOffset dateTime, double expirationTimeInHours, CancellationToken cancellationToken)
    {
        var baseCurrency = GetBaseCurrencyFromDb();
        var currenciesRates = await _currencyRateDbContext.Currencies
            .Where(c => c.BaseCurrency == baseCurrency)
            .OrderByDescending(c => c.DateTime)
            .FirstOrDefaultAsync(cancellationToken);
        if (currenciesRates != null)
        {
            var timeDiff = dateTime - currenciesRates.DateTime;
            if (timeDiff.TotalHours < expirationTimeInHours)
                return Array.ConvertAll(currenciesRates.Currencies, ConvertCurrencyRateToCurrency);
        }
        var tasksInProgress = await GetFromDbTasksInProgress(cancellationToken);
        if (tasksInProgress.Length > 0)
            await Task.Delay(10_000, cancellationToken);
        var tasksInProgressAfterDelay = await GetFromDbTasksInProgress(cancellationToken);
        if (tasksInProgressAfterDelay.Length > 0)
            throw new Exception("Очередь задач не движется!");
        return null;
    }

    private async Task<Array> GetFromDbTasksInProgress(CancellationToken cancellationToken)
        => await _currencyRateDbContext.CacheTasks
            .Where(p =>
                p.Status == CacheTaskStatus.Created ||
                p.Status == CacheTaskStatus.InProgress)
            .ToArrayAsync(cancellationToken);
    
    /// <summary>
    /// Получение из базы данных курсов валют на дату
    /// </summary>
    /// <param name="dateTime">Дата, на которую нужно получить курсы валют</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Если данные в базе данных есть, то массив курсов валют. Иначе null</returns>
    private async Task<Currency[]?> GetDataFromDb(DateTimeOffset dateTime, CancellationToken cancellationToken)
    {
        var baseCurrency = GetBaseCurrencyFromDb();
        var currenciesRates = await _currencyRateDbContext.Currencies
            .Where(c =>
                c.BaseCurrency == baseCurrency &&
                DateOnly.FromDateTime(c.DateTime.Date) == DateOnly.FromDateTime(dateTime.Date))
            .OrderByDescending(c => c.DateTime)
            .FirstOrDefaultAsync(cancellationToken);
        return currenciesRates == null ? null : Array.ConvertAll(currenciesRates.Currencies, ConvertCurrencyRateToCurrency);
    }

    /// <summary>
    /// Запись в базу данных массива курсов валют на дату
    /// </summary>
    /// <param name="currencies">Массив курсов валют</param>
    /// <param name="date">Дата актуальности курсов</param>
    private void CacheCurrenciesOnDateAsync(Currency[] currencies, DateTimeOffset date)
    {
        var baseCurrency = GetBaseCurrencyFromDb();
        var currenciesRates = new CurrenciesRates
        {
            BaseCurrency = baseCurrency,
            Currencies = Array.ConvertAll(currencies, ConvertCurrencyToCurrencyRate),
            DateTime = date
        };
        _currencyRateDbContext.Add(currenciesRates);
        _currencyRateDbContext.SaveChanges();
    }

    private static Currency ConvertCurrencyRateToCurrency(CurrencyRate currencyRate)
        => new (currencyRate.Code, currencyRate.Value);
    
    private static CurrencyRate ConvertCurrencyToCurrencyRate(Currency currency) 
        => new() {Code = currency.Code, Value = currency.Value};

    private string GetBaseCurrencyFromDb()
    {
        var baseCurrency = _currencyRateDbContext.Settings.FirstOrDefault();
        if (baseCurrency == null)
        {
            throw new CurrencySettingsNotFoundException("Базовой валюты не найдено");
        }

        return baseCurrency.DefaultCurrency;
    }
}