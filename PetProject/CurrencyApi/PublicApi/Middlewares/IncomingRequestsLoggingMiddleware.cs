namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Middlewares;

public class IncomingRequestsLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IncomingRequestsLoggingMiddleware> _logger;

    public IncomingRequestsLoggingMiddleware(RequestDelegate next, ILogger<IncomingRequestsLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        _logger.LogInformation("Requst Method: {RequestMethod}, Request Http Context: {RequestHttpContext}", context.Request.Method, context.Request.HttpContext);
        await _next(context);
    }
}