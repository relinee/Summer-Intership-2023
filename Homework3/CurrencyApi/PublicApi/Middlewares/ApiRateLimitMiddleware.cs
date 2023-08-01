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
        var apiStatus = await _currencyService.CheckApiStatusAsync();
        var countRequest = apiStatus.Quotas.Month.Remaining; 
        if (countRequest == 0)
            throw new ApiRequestLimitException("Больше запросов нет :(");
        await _next(context);
    }
    
}