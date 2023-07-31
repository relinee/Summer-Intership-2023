namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Services;

public interface ICurrencyService
{
    public Task<HttpResponseMessage> SendRequestToGetStatusAsync();

    public Task<HttpResponseMessage> SendRequestToGetCurrencyRateAsync(string baseCurrencyCode,
        string defaultCurrencyCode);

    public Task<HttpResponseMessage> SendRequestToGetHistoricalCurrencyRateAsync(string baseCurrencyCode,
        string defaultCurrencyCode, DateOnly date);
    
}