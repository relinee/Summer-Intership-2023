using Fuse8_ByteMinds.SummerSchool.InternalApi.Contracts;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Exceptions;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Config;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Enum = System.Enum;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Controllers;

/// <summary>
/// Методы для получения курсов валют
/// </summary>
[Route("currency")]
public class CurrencyController : Controller
{
    private readonly ICachedCurrencyAPI _cachedCurrencyApi;
    private readonly ICurrencyAPI _currencyApi;
    private readonly IOptionsMonitor<ApiSettings> _apiSettings;

    public CurrencyController(
        ICachedCurrencyAPI cachedCurrencyApi,
        ICurrencyAPI currencyApi,
        IOptionsMonitor<ApiSettings> apiSettings)
    {
        _cachedCurrencyApi = cachedCurrencyApi;
        _currencyApi = currencyApi;
        _apiSettings = apiSettings;
    }

    /// <summary>
    /// Получить курс для валюты относительно базовой
    /// </summary>
    /// <param name="currencyType">Валюта для подсчета курса</param>
    /// <response code="200">
    /// Возвращает если удалось получить курс валюты
    /// </response>
    /// <response code="429">
    /// Возвращает если превышено ограничение по количеству запросов
    /// </response>
    /// <response code="404">
    /// Возвращает если не удалось получить курс для текующей валюты
    /// </response>
    /// <response code="500">
    /// Возвращает если произошла ошибка на сервере
    /// </response>
    [HttpGet]
    [Route("{currencyType}")]
    public async Task<ActionResult<CurrencyDTO>> GetCurrentCurrencyRate(CurrencyType currencyType)
    {
        var cancelTokenSource = new CancellationTokenSource();
        var cancellationToken = cancelTokenSource.Token;
        await HealthCheck(cancellationToken);
        var currencyRate = await _cachedCurrencyApi.GetCurrentCurrencyAsync(
            currencyType: currencyType,
            cancellationToken: cancellationToken);
        return currencyRate;
    }

    /// <summary>
    /// Получить курс для валюты по выбору на дату 
    /// </summary>
    /// <param name="currencyType">Валюта для подсчета курса</param>
    /// <param name="dateTime">Дата в формате YYYY-MM-DD</param>
    /// <response code="200">
    /// Возвращает если удалось получить курс валюты
    /// </response>
    /// <response code="429">
    /// Возвращает если превышено ограничение по количеству запросов
    /// </response>
    /// <response code="404">
    /// Возвращает если не удалось получить курс для текующей валюты
    /// </response>
    /// <response code="500">
    /// Возвращает если произошла ошибка на сервере
    /// </response>
    [HttpGet]
    [Route("{currencyType}/{dateTime}")]
    public async Task<ActionResult<CurrencyDTO>> GetCurrencyRateOnDate(CurrencyType currencyType, DateTime dateTime)
    {
        var cancelTokenSource = new CancellationTokenSource();
        var cancellationToken = cancelTokenSource.Token;
        await HealthCheck(cancellationToken);
        var currencyRate = await _cachedCurrencyApi.GetCurrencyOnDateAsync(
            currencyType: currencyType,
            date: DateOnly.FromDateTime(dateTime),
            cancellationToken: cancellationToken);
        return currencyRate;
    }
    
    /// <summary>
    /// Получить настроки приложения
    /// </summary>
    /// <response code="200">
    /// Возвращает если удалось получить настройки
    /// </response>
    /// <response code="429">
    /// Возвращает если превышено ограничение по количеству запросов
    /// </response>
    /// <response code="500">
    /// Возвращает если произошла ошибка на сервере
    /// </response>
    [HttpGet("settings")]
    public async Task<ActionResult<CurrencySettings>> GetSettings()
    {
        var cancelTokenSource = new CancellationTokenSource();
        var cancellationToken = cancelTokenSource.Token;
        var baseCurrencyCode = _apiSettings.CurrentValue.BaseCurrency;
        var convertedBaseCurrCode =
            $"{baseCurrencyCode[0]}{char.ToLower(baseCurrencyCode[1])}{char.ToLower(baseCurrencyCode[2])}";
        var isNewRequestAvailable = await _currencyApi.IsNewRequestsAvailable(cancellationToken);
        return new CurrencySettings(Enum.Parse<CurrencyType>(convertedBaseCurrCode), isNewRequestAvailable);
    }

    private async Task HealthCheck(CancellationToken cancellationToken)
    {
        var apiStatus = await _currencyApi.IsNewRequestsAvailable(cancellationToken);
        if (!apiStatus)
            throw new ApiRequestLimitException("Больше запросов нет :(");
    }
}