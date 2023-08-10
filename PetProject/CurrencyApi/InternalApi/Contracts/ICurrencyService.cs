using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response.ExternalAPI;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Contracts;

public interface ICurrencyService
{
    public Task<LatestExchangeRates> GetCurrencyRateAsync(string currencyCode);

    public Task<HistoricalExchangeRates> GetHistoricalCurrencyRateAsync(string currencyCode, DateOnly date);

    public Task<SettingsResult> GetSettingsAsync();

    public Task<ApiStatus> CheckApiStatusAsync();
}