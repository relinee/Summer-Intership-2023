using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;
using Microsoft.Extensions.Options;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Services;

public class CurrencyService : ICurrencyService
{
    private readonly HttpClient _httpClient;
    private readonly IOptionsMonitor<ApiSettings> _apiSettingsAsOptionsMonitor;
    private const string BaseUrl = "https://api.currencyapi.com/v3";

    public CurrencyService(HttpClient httpClient, IOptionsMonitor<ApiSettings> apiSettingsAsOptionsMonitor)
    {
        _httpClient = httpClient;
        _apiSettingsAsOptionsMonitor = apiSettingsAsOptionsMonitor;
    }

    public Task<HttpResponseMessage> SendRequestToGetStatusAsync()
    {
        return SendRequestWithPath(BaseUrl + "/status");
    }
    
    public Task<HttpResponseMessage> SendRequestToGetCurrencyRateAsync(string baseCurrencyCode, string defaultCurrencyCode)
    {
        return SendRequestWithPath(BaseUrl + $"/latest?currencies={defaultCurrencyCode}&base_currency={baseCurrencyCode}");
    }
    
    public Task<HttpResponseMessage> SendRequestToGetHistoricalCurrencyRateAsync(string baseCurrencyCode, string defaultCurrencyCode, DateOnly date)
    {
        return SendRequestWithPath(BaseUrl + $"/historical?currencies={defaultCurrencyCode}&date={date.ToString("yyyy-MM-dd")}&base_currency={baseCurrencyCode}");
    }

    private async Task<HttpResponseMessage> SendRequestWithPath(string path)
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("apikey", _apiSettingsAsOptionsMonitor.CurrentValue.ApiKey);
        return await _httpClient.GetAsync(path);
    }
    
    
}