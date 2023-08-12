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
        var cachedCurrencies = GetCachedDataWithoutExpirationTime($"{DateTime.UtcNow:yyyy_MM_dd}");
        if (cachedCurrencies != null) return GetCurrentCurrencyDto(cachedCurrencies, currencyType);
        
        var currentCurrencies = await _currencyApi.GetAllCurrentCurrenciesAsync(_apiSettings.CurrentValue.BaseCurrency, cancellationToken);
        await CacheCurrenciesAsync(currencies: currentCurrencies);

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

    private static CurrencyDTO GetCurrentCurrencyDto(Currency[] currencies, CurrencyType currencyType)
    {
        var currentCurrencyValue = currencies.First(c => c.Code == currencyType.ToString().ToUpperInvariant()).Value;
        return new CurrencyDTO(currencyType, currentCurrencyValue);
    }

    private Currency[]? GetCachedDataWithoutExpirationTime(string date)
    {
        var cacheFilename = FindCacheFilenameByDate(date);
        if (cacheFilename == null) return null;
        var dateTimeFromFilename = DateTime.ParseExact(Path.GetFileNameWithoutExtension(cacheFilename), DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
        var timeDiff = DateTime.UtcNow - dateTimeFromFilename;
        return timeDiff.TotalHours < _cacheSettings.CurrentValue.ExpirationTimeInHours
            ? LoadDataFromCacheFile(cacheFilename) : null;
    }
    
    private Currency[]? GetCachedData(string date)
    {
        var cacheFilename = FindCacheFilenameByDate(date);
        return cacheFilename == null ? null : LoadDataFromCacheFile(cacheFilename);
    }
    
    private string? FindCacheFilenameByDate(string date)
    {
        if (!Directory.Exists(_cacheSettings.CurrentValue.Directory)) return null;

        var cacheFiles = Directory.GetFiles(_cacheSettings.CurrentValue.Directory, $"{date}*.json");
        if (cacheFiles.Length <= 0) return null;
        
        var maxTimestampFilename = cacheFiles.OrderByDescending(ExtractDateTimeFromFilename).First();
        return maxTimestampFilename;
    }
    
    private static DateTime ExtractDateTimeFromFilename(string filename)
    {
        var filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
        return DateTime.TryParseExact(filenameWithoutExtension, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None,  out DateTime dateTime) ? dateTime : DateTime.MinValue;
    }
    
    private static Currency[]? LoadDataFromCacheFile(string cacheFilename)
    {
        if (!File.Exists(cacheFilename)) return null;
        var json = File.ReadAllText(cacheFilename);
        return JsonSerializer.Deserialize<Currency[]>(json);
    }

    private async Task CacheCurrenciesAsync(Currency[] currencies)
    {
        if (!Directory.Exists(_cacheSettings.CurrentValue.Directory))
        {
            Directory.CreateDirectory(_cacheSettings.CurrentValue.Directory);
        }

        var cacheFilename = Path.Combine(
            _cacheSettings.CurrentValue.Directory,
            $"{DateTime.UtcNow.ToString(DateFormat)}.json");
        var json = JsonSerializer.Serialize(currencies);
        await File.WriteAllTextAsync(cacheFilename, json);
    }
    
    private async Task CacheCurrenciesOnDateAsync(Currency[] currencies, DateTime date)
    {
        if (!Directory.Exists(_cacheSettings.CurrentValue.Directory))
        {
            Directory.CreateDirectory(_cacheSettings.CurrentValue.Directory);
        }
        var cacheFilename = Path.Combine(
            _cacheSettings.CurrentValue.Directory,
            $"{date.ToString(DateFormat)}.json");
        var json = JsonSerializer.Serialize(currencies);
        await File.WriteAllTextAsync(cacheFilename, json);
    }
}