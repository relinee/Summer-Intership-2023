using Fuse8_ByteMinds.SummerSchool.PublicApi.Contracts;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Exceptions;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Middlewares;

public class ApiRateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICurrencyService _currencyService;
    
    public ApiRateLimitMiddleware(RequestDelegate next, ICurrencyService currencyService)
    {
        _next = next;
        _currencyService = currencyService;
    }

    public async Task Invoke(HttpContext context)
    {
        var apiStatus = await _currencyService.GetSettingsAsync();
        if (!apiStatus.NewRequestsAvailable)
            throw new ApiRequestLimitException("Больше запросов нет :(");
        await _next(context);
    }
    
}