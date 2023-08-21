using System.Globalization;
using System.Text.Json;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Contracts;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Config;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response;
using Microsoft.Extensions.Options;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services;

internal class CachedCurrencyService : ICachedCurrencyAPI
{
    
    private readonly ICurrencyAPI _currencyApi;
    private readonly IOptionsMonitor<CurrencyCacheSettings> _cacheSettings;
    private readonly IOptionsMonitor<ApiSettings> _apiSettings;
    private const string DateFormat = "yyyy_MM_dd_HH_mm_ss";

    public CachedCurrencyService(
        ICurrencyAPI currencyApi,
        IOptionsMonitor<CurrencyCacheSettings> cacheSettings,
        IOptionsMonitor<ApiSettings> apiSettings)
    {
        _currencyApi = currencyApi;
        _cacheSettings = cacheSettings;
        _apiSettings = apiSettings;
    }
    
    public async Task<CurrencyDTO> GetCurrentCurrencyAsync(CurrencyType currencyType, CancellationToken cancellationToken)
    {
        var cachedCurrencies = GetCachedDataWithoutExpirationTime(
            date: $"{DateTimeOffset.UtcNow:yyyy_MM_dd}",
            expirationTimeInHours: _cacheSettings.CurrentValue.ExpirationTimeInHours);
        if (cachedCurrencies != null) return GetCurrentCurrencyDto(cachedCurrencies, currencyType);
        
        var currentCurrencies = await _currencyApi.GetAllCurrentCurrenciesAsync(_apiSettings.CurrentValue.BaseCurrency, cancellationToken);
        await CacheCurrenciesOnDateAsync(
            currencies: currentCurrencies,
            date: DateTimeOffset.UtcNow);

        return GetCurrentCurrencyDto(currentCurrencies, currencyType);
    }
    
    public async Task<CurrencyDTO> GetCurrencyOnDateAsync(CurrencyType currencyType, DateOnly date, CancellationToken cancellationToken)
    {
        var cachedCurrencies = GetCachedData($"{date:yyyy_MM_dd}");
        if (cachedCurrencies != null) return GetCurrentCurrencyDto(cachedCurrencies, currencyType);
        
        var currenciesRateOnDate = await _currencyApi.GetAllCurrenciesOnDateAsync(_apiSettings.CurrentValue.BaseCurrency, date, cancellationToken);
        await CacheCurrenciesOnDateAsync(
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
    /// Получение из кеша курсов валют без истечения срока давности
    /// </summary>
    /// <param name="date">Дата, на которую нужен курс валют</param>
    /// <param name="expirationTimeInHours">Срок актуальности кеша в часах</param>
    /// <returns>Если срок давности не вышел и данные в кеше есть, то массив курсов валют. Иначе null</returns>
    private Currency[]? GetCachedDataWithoutExpirationTime(string date, double expirationTimeInHours)
    {
        var cacheFilename = FindCacheFilenameByDate(date);
        if (cacheFilename == null) return null;
        var dateTimeFromFilename = DateTimeOffset.ParseExact(
            Path.GetFileNameWithoutExtension(cacheFilename)[4..],
            DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
        var timeDiff = DateTimeOffset.UtcNow - dateTimeFromFilename;
        return timeDiff.TotalHours < expirationTimeInHours
            ? LoadDataFromCacheFile(cacheFilename) : null;
    }
    
    /// <summary>
    /// Получение из кеша курсов валют на дату
    /// </summary>
    /// <param name="date">Дата, на которую нужно получить курсы валют</param>
    /// <returns>Если данные в кеше есть, то массив курсов валют. Иначе null</returns>
    private Currency[]? GetCachedData(string date)
    {
        var cacheFilename = FindCacheFilenameByDate(date);
        return cacheFilename == null ? null : LoadDataFromCacheFile(cacheFilename);
    }
    
    /// <summary>
    /// Найти максимальное по времени название файла в кеше по дате
    /// </summary>
    /// <param name="date">Дата для поиска кеша</param>
    /// <returns>Если файлы с такой датой и базовой валютой есть, то максимальное по времени название файла в кеше. Иначе null</returns>
    private string? FindCacheFilenameByDate(string date)
    {
        if (!Directory.Exists(_cacheSettings.CurrentValue.Directory)) return null;

        var cacheFiles = Directory.GetFiles(
            _cacheSettings.CurrentValue.Directory,
            $"{_apiSettings.CurrentValue.BaseCurrency}_{date}*.json");
        if (cacheFiles.Length <= 0) return null;
        
        var maxTimestampFilename = cacheFiles.OrderByDescending(ExtractDateTimeFromFilename).First();
        return maxTimestampFilename;
    }
    
    // TODO : перенести/изменить
    /// <summary>
    /// Получение DateTime из названия файла
    /// </summary>
    /// <param name="filename">Название файла</param>
    /// <returns>Если из названия файла получилось извлечь дату, то дата из него. Иначе null</returns>
    private static DateTimeOffset? ExtractDateTimeFromFilename(string filename)
    {
        var filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
        return DateTimeOffset.TryParseExact(filenameWithoutExtension, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None,  out DateTimeOffset dateTime) ? dateTime : null;
    }
    
    /// <summary>
    /// Получение курсов валют из файла
    /// </summary>
    /// <param name="cacheFilename">Название файла</param>
    /// <returns>Если файл с таким названием есть, то массив курсов валют. Иначе null</returns>
    private static Currency[]? LoadDataFromCacheFile(string cacheFilename)
    {
        if (!File.Exists(cacheFilename)) return null;
        var json = File.ReadAllText(cacheFilename);
        return JsonSerializer.Deserialize<Currency[]>(json);
    }

    /// <summary>
    /// Запись в файл массива курсов валют на дату
    /// </summary>
    /// <param name="currencies">Массив курсов валют</param>
    /// <param name="date">Дата актуальности курсов</param>
    private async Task CacheCurrenciesOnDateAsync(Currency[] currencies, DateTimeOffset date)
    {
        if (!Directory.Exists(_cacheSettings.CurrentValue.Directory))
        {
            Directory.CreateDirectory(_cacheSettings.CurrentValue.Directory);
        }
        var cacheFilename = Path.Combine(
            _cacheSettings.CurrentValue.Directory,
            $"{_apiSettings.CurrentValue.BaseCurrency}_{date.ToString(DateFormat)}.json");
        var json = JsonSerializer.Serialize(currencies);
        await File.WriteAllTextAsync(cacheFilename, json);
    }
}