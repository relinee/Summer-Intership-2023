using System.Net;
using System.Text.Json.Serialization;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Controllers;

/// <summary>
/// Методы для получения курсов валют
/// </summary>
[Route("currency")]
public class CurrencyController : ControllerBase
{
    private readonly IOptionsMonitor<ApiSettings> _apiSettingsAsOptionsMonitor;
    private readonly ICurrencyService _currencyService;
    
    public CurrencyController(IOptionsMonitor<ApiSettings> apiSettingsAsOptionsMonitor, CurrencyService currencyService)
    {
        _apiSettingsAsOptionsMonitor = apiSettingsAsOptionsMonitor;
        _currencyService = currencyService;
    }

    /// <summary>
    /// Получить курс для валюты по умолчанию
    /// </summary>
    /// <response code="200">
    /// Возвращает если удалось получить курс валют
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
    public async Task<ActionResult<LatestExchangeRates>> GetDefaultCurrencyRate()
    {
        return await _currencyService.GetCurrencyRateAsync(_apiSettingsAsOptionsMonitor.CurrentValue.DefaultCurrency);
    }


    /// <summary>
    /// Получить курс для валюты по выбору 
    /// </summary>
    /// /// <param name="currencyCode"> Валюта по выбору </param>
    /// <response code="200">
    /// Возвращает если удалось получить курс валют
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
    [Route("{currencyCode}")]
    public async Task<ActionResult<LatestExchangeRates>> GetNotDefaultCurrencyRate(string currencyCode)
    {
        return await _currencyService.GetCurrencyRateAsync(currencyCode);
    }

    /// <summary>
    /// Получить курс для валюты по выбору в определенный день
    /// </summary>
    /// <param name="currencyCode"> Валюта по выбору </param>
    /// <param name="date"> Дата в формате YYYY-MM-DD </param>
    /// <response code="200">
    /// Возвращает если удалось получить курс валют
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
    [Route("{currencyCode}/{date}")]
    public async Task<ActionResult<HistoricalExchangeRates>> GetHistoricalCurrencyRate(string currencyCode, DateOnly date)
    {
        return await _currencyService.GetHistoricalCurrencyRateAsync(currencyCode, date);
    }

    /// <summary>
    /// Получить настроки приложения
    /// </summary>
    /// <param name="currencyCode"> Валюта по выбору </param>
    /// <response code="200">
    /// Возвращает если удалось получить курс валют
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
    [HttpGet("settings")]
    public async Task<ActionResult<SettingsResult>> GetSettings()
    {
        return await _currencyService.GetSettingsAsync();
    }
}

/// <summary>
/// Последний курс для валюты
/// </summary>
public record LatestExchangeRates
{
    /// <summary>
    /// Код валюты
    /// </summary>
    [JsonPropertyName("code")]
    public string ValueCode { get; init; }
    
    /// <summary>
    /// Текущий курс относительно валюты по умолчанию
    /// </summary>
    [JsonPropertyName("value")]
    public decimal CurrentValueRate { get; init; }
    
}

/// <summary>
/// Курс для валюты в определенный день
/// </summary>
public record HistoricalExchangeRates
{
    /// <summary>
    /// Дата актуальности курса
    /// </summary>
    [JsonPropertyName("date")]
    public DateOnly Date { get; init; }
    
    /// <summary>
    /// Код валюты
    /// </summary>
    [JsonPropertyName("code")]
    public string ValueCode { get; init; }
    
    /// <summary>
    /// Текущий курс относительно валюты по умолчанию
    /// </summary>
    [JsonPropertyName("value")]
    public decimal CurrentValueRate { get; init; }
    
}

/// <summary>
/// Настройки приложения
/// </summary>
public record SettingsResult
{
    /// <summary>
    /// Текущий курс валют по умолчанию из конфигурации
    /// </summary>
    [JsonPropertyName("defaultCurrency")]
    public string DefaultCurrency { get; init; }
    
    /// <summary>
    /// Базовая валюта, относительно которой считается курс
    /// </summary>
    [JsonPropertyName("baseCurrency")]
    public string BaseCurrency { get; init; }
    
    /// <summary>
    /// Общее количество доступных запросов, полученное от внешнего API (quotas->month->total)
    /// </summary>
    [JsonPropertyName("requestLimit")]
    public int RequestLimit { get; init; }
    
    /// <summary>
    /// Количество использованных запросов, полученное от внешнего API (quotas->month->used)
    /// </summary>
    [JsonPropertyName("requestCount")]
    public int RequestCount { get; init; }
    
    /// <summary>
    /// Количество знаков после запятой, до которого следует округлять значение курса валют
    /// </summary>
    [JsonPropertyName("currencyRoundCount")]
    public int CurrencyRoundCount { get; init; }
}
