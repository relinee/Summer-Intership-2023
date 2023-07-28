using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Middleware;

public class ApiRateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    // public ApiRateLimitMiddleware(RequestDelegate next, HttpClient httpClient, IConfiguration configuration)
    // {
    //     _next = next;
    //     _httpClient = httpClient;
    //     _configuration = configuration;
    // }
    
    public ApiRateLimitMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _next = next;
        _httpClient = httpClientFactory.CreateClient("AuditHttpClient");;
        _configuration = configuration;
    }

    public async Task Invoke(HttpContext context)
    {
        var countRequest = await GetCountRequest();
        if (countRequest == 0)
            throw new ApiRequestLimitException("Больше запросов нет :(");
        await _next(context);
    }

    private async Task<int> GetCountRequest()
    {
        var currUrl = "https://api.currencyapi.com/v3/status";
        var apiSettings =  _configuration.GetSection("ApiSettings").Get<ApiSettings>();
        
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("apikey", apiSettings.ApiKey);
        
        var response = await _httpClient.GetAsync(currUrl);
        if (!response.IsSuccessStatusCode)
            throw new Exception("Ошибка получения количества доступных запросов!");
        var apiStatus = await response.Content.ReadFromJsonAsync<ApiStatus>();
        if (apiStatus == null)
            throw new Exception("Ошибка преобразования данных");
        return apiStatus.Quotas.Month.Remaining;
    }
}