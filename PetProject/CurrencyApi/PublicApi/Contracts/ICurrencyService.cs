using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Response;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Contracts;

public interface ICurrencyService
{
    public Task<ExchangeRates> GetCurrencyRateAsync(CurrencyType currencyCode);

    public Task<ExchangeRatesWithDate> GetHistoricalCurrencyRateAsync(CurrencyType currencyCode, DateOnly date);

    public Task<SettingsResult> GetSettingsAsync();
}