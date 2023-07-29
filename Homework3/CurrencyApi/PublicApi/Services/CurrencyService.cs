using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Services;

public class CurrencyService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private const string BaseUrl = "https://api.currencyapi.com/v3";

    public CurrencyService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
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
        var apiSettings =  _configuration.GetSection("ApiSettings").Get<ApiSettings>();
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("apikey", apiSettings.ApiKey);
        return await _httpClient.GetAsync(path);
    }
    
    
}