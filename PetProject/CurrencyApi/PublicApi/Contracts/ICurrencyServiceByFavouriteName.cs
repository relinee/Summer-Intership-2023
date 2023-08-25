using Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Response;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Contracts;

public interface ICurrencyServiceByFavouriteName
{
    public Task<ExchangeRates> GetCurrencyRateByFavouriteNameAsync(string name, CancellationToken cancellationToken);
    
    public Task<ExchangeRatesWithDate> GetCurrencyRateOnDateByFavouriteNameAsync(string name, DateOnly dateOnly, CancellationToken cancellationToken);
}