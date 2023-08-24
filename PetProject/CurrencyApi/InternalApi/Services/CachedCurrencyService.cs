using Fuse8_ByteMinds.SummerSchool.InternalApi.Contracts;
using Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts;
using Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts.Entities;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Config;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response;
using Microsoft.Extensions.Options;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services;

internal class CachedCurrencyService : ICachedCurrencyAPI
{
    
    private readonly ICurrencyAPI _currencyApi;
    private readonly IOptionsMonitor<CurrencyCacheSettings> _cacheSettings;
    private readonly IOptionsMonitor<ApiSettings> _apiSettings;
    private readonly CurrencyRateDbContext _currencyRateDbContext;

    public CachedCurrencyService(
        ICurrencyAPI currencyApi,
        IOptionsMonitor<CurrencyCacheSettings> cacheSettings,
        IOptionsMonitor<ApiSettings> apiSettings,
        CurrencyRateDbContext currencyRateDbContext)
    {
        _currencyApi = currencyApi;
        _cacheSettings = cacheSettings;
        _apiSettings = apiSettings;
        _currencyRateDbContext = currencyRateDbContext;
    }
    
    public async Task<CurrencyDTO> GetCurrentCurrencyAsync(CurrencyType currencyType, CancellationToken cancellationToken)
    {
        var cachedCurrencies = GetDataFromDbWithoutExpirationTime(
            dateTime: DateTimeOffset.UtcNow,
            expirationTimeInHours: _cacheSettings.CurrentValue.ExpirationTimeInHours);
        if (cachedCurrencies != null) return GetCurrentCurrencyDto(cachedCurrencies, currencyType);
        
        var currentCurrencies = await _currencyApi.GetAllCurrentCurrenciesAsync(_apiSettings.CurrentValue.BaseCurrency, cancellationToken);
        
        CacheCurrenciesOnDateAsync(
            currencies: currentCurrencies,
            date: DateTimeOffset.UtcNow);

        return GetCurrentCurrencyDto(currentCurrencies, currencyType);
    }
    
    public async Task<CurrencyDTO> GetCurrencyOnDateAsync(CurrencyType currencyType, DateOnly date, CancellationToken cancellationToken)
    {
        var cachedCurrencies = GetDataFromDb(date.ToDateTime(TimeOnly.MaxValue));
        if (cachedCurrencies != null) return GetCurrentCurrencyDto(cachedCurrencies, currencyType);
        
        var currenciesRateOnDate = await _currencyApi.GetAllCurrenciesOnDateAsync(_apiSettings.CurrentValue.BaseCurrency, date, cancellationToken);
        
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
    /// <returns>Если срок давности не вышел и данные в базе данных есть, то массив курсов валют. Иначе null</returns>
    private Currency[]? GetDataFromDbWithoutExpirationTime(DateTimeOffset dateTime, double expirationTimeInHours)
    {
        var currenciesRates = _currencyRateDbContext.Currencies
            .Where(c => c.BaseCurrency == _apiSettings.CurrentValue.BaseCurrency)
            .OrderByDescending(c => c.DateTime)
            .FirstOrDefault();
        if (currenciesRates == null) return null;
        
        var timeDiff = dateTime - currenciesRates.DateTime;
        return timeDiff.TotalHours < expirationTimeInHours
            ? Array.ConvertAll(currenciesRates.Currencies, ConvertCurrencyRateToCurrency) : null;
    }

    /// <summary>
    /// Получение из базы данных курсов валют на дату
    /// </summary>
    /// <param name="dateTime">Дата, на которую нужно получить курсы валют</param>
    /// <returns>Если данные в базе данных есть, то массив курсов валют. Иначе null</returns>
    private Currency[]? GetDataFromDb(DateTimeOffset dateTime)
    {
        var currenciesRates = _currencyRateDbContext.Currencies
            .Where(c =>
                c.BaseCurrency == _apiSettings.CurrentValue.BaseCurrency &&
                DateOnly.FromDateTime(c.DateTime.Date) == DateOnly.FromDateTime(dateTime.Date))
            .OrderByDescending(c => c.DateTime)
            .FirstOrDefault();
        return currenciesRates == null ? null : Array.ConvertAll(currenciesRates.Currencies, ConvertCurrencyRateToCurrency);
    }

    /// <summary>
    /// Запись в базу данных массива курсов валют на дату
    /// </summary>
    /// <param name="currencies">Массив курсов валют</param>
    /// <param name="date">Дата актуальности курсов</param>
    private void CacheCurrenciesOnDateAsync(Currency[] currencies, DateTimeOffset date)
    {
        var currenciesRates = new CurrenciesRates
        {
            BaseCurrency = _apiSettings.CurrentValue.BaseCurrency,
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
}