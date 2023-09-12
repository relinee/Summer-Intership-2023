using Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Response;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Contracts;

public interface ICurrencyService
{
    public Task<ExchangeRates> GetCurrencyRateAsync(CurrencyType currencyCode, CancellationToken cancellationToken);

    public Task<ExchangeRatesWithDate> GetHistoricalCurrencyRateAsync(CurrencyType currencyCode, DateOnly date, CancellationToken cancellationToken);

    public Task<SettingsResult> GetSettingsAsync(CancellationToken cancellationToken);
}