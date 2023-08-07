using Fuse8_ByteMinds.SummerSchool.PublicApi.Controllers;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Services;

public interface ICurrencyService
{
    public Task<LatestExchangeRates> GetCurrencyRateAsync(string currencyCode);

    public Task<HistoricalExchangeRates> GetHistoricalCurrencyRateAsync(string currencyCode, DateOnly date);

    public Task<SettingsResult> GetSettingsAsync();

    public Task<ApiStatus> CheckApiStatusAsync();
}