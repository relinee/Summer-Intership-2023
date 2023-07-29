using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Services;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Middleware;

public class ApiRateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly CurrencyService _currencyService;
    
    public ApiRateLimitMiddleware(RequestDelegate next, CurrencyService currencyService)
    {
        _next = next;
        _currencyService = currencyService;
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
        var response = await _currencyService.SendRequestToGetStatusAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception("Ошибка получения количества доступных запросов!");
        
        var apiStatus = await response.Content.ReadFromJsonAsync<ApiStatus>();
        if (apiStatus == null)
            throw new Exception("Ошибка преобразования данных");
        return apiStatus.Quotas.Month.Remaining;
    }
}