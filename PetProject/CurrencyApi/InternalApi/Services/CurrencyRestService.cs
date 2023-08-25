using Fuse8_ByteMinds.SummerSchool.InternalApi.Contracts;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Exceptions;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Config;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response;
using Microsoft.Extensions.Options;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services;

public class CurrencyRestService : ICurrencyRestService
{
    
    private readonly ICachedCurrencyAPI _cachedCurrencyApi;
    private readonly ICurrencyAPI _currencyApi;
    private readonly IOptionsMonitor<ApiSettings> _apiSettings;

    public CurrencyRestService(
        ICachedCurrencyAPI cachedCurrencyApi,
        ICurrencyAPI currencyApi,
        IOptionsMonitor<ApiSettings> apiSettings)
    {
        _cachedCurrencyApi = cachedCurrencyApi;
        _currencyApi = currencyApi;
        _apiSettings = apiSettings;
    }
    
    public async Task<CurrencyDTO> GetCurrentCurrencyRateAsync(CurrencyType currencyType, CancellationToken cancellationToken)
    {
        var currencyRate = await _cachedCurrencyApi.GetCurrentCurrencyAsync(
            currencyType: currencyType,
            cancellationToken: cancellationToken);
        return currencyRate;
    }
    
    public async Task<CurrencyDTO> GetCurrencyRateOnDateAsync(CurrencyType currencyType, DateOnly date, CancellationToken cancellationToken)
    {
        var currencyRate = await _cachedCurrencyApi.GetCurrencyOnDateAsync(
            currencyType: currencyType,
            date: date,
            cancellationToken: cancellationToken);
        return currencyRate;
    }

    public async Task<CurrencySettings> GetSettingsAsync(CancellationToken cancellationToken)
    {
        var baseCurrencyCode = _apiSettings.CurrentValue.BaseCurrency;
        var convertedBaseCurrCode =
            $"{baseCurrencyCode[0]}{char.ToLower(baseCurrencyCode[1])}{char.ToLower(baseCurrencyCode[2])}";
        var isNewRequestAvailable = await _currencyApi.IsNewRequestsAvailable(cancellationToken);
        return new CurrencySettings(Enum.Parse<CurrencyType>(convertedBaseCurrCode), isNewRequestAvailable);
    }
}