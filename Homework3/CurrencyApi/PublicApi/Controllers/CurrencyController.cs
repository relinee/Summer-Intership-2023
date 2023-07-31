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
        return await GetCurrencyRate(_apiSettingsAsOptionsMonitor.CurrentValue.DefaultCurrency);
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
        return await GetCurrencyRate(currencyCode);
    }

    private async Task<LatestExchangeRates> GetCurrencyRate(string currencyCode)
    {
        var response = await _currencyService.SendRequestToGetCurrencyRateAsync(_apiSettingsAsOptionsMonitor.CurrentValue.BaseCurrency, currencyCode);
        if (response.IsSuccessStatusCode)
        {
            var latestCurrencyExchangeData = await response.Content.ReadFromJsonAsync<CurrencyExchangeData>();
            if (latestCurrencyExchangeData?.Data == null || latestCurrencyExchangeData.Meta == null)
                throw new Exception("Ошибка преобразования данных");
            var roundedValue = Math.Round(latestCurrencyExchangeData.Data[currencyCode].Value, _apiSettingsAsOptionsMonitor.CurrentValue.DecimalPlaces);
            return new LatestExchangeRates
            {
                ValueCode = latestCurrencyExchangeData.Data[currencyCode].Code,
                CurrentValueRate = roundedValue
            };
        }

        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            var content = await response.Content.ReadFromJsonAsync<ErrorApiResponse>();
            if (content != null && content.Errors.Currencies.Contains("The selected currencies is invalid."))
                throw new CurrencyNotFoundException("Неизвестная валюта");
        }
        throw new Exception("Ошибка выполнения запроса");
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
        // if (date == DateOnly.Parse("0001-01-01"))
        //     throw new Exception("Ошибка преобразования даты");
        var response = await _currencyService.SendRequestToGetHistoricalCurrencyRateAsync(_apiSettingsAsOptionsMonitor.CurrentValue.BaseCurrency, currencyCode, date);
        if (response.IsSuccessStatusCode)
        {
            var historicalCurrencyExchangeData = await response.Content.ReadFromJsonAsync<CurrencyExchangeData>();
            if (historicalCurrencyExchangeData?.Data == null || historicalCurrencyExchangeData.Meta == null)
                throw new Exception("Ошибка преобразования данных");
            var roundedValue = Math.Round(historicalCurrencyExchangeData.Data[currencyCode].Value, _apiSettingsAsOptionsMonitor.CurrentValue.DecimalPlaces);
            return new HistoricalExchangeRates
            {
                Date = date,
                ValueCode = historicalCurrencyExchangeData.Data[currencyCode].Code,
                CurrentValueRate = roundedValue
                
            };
        }

        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            var content = await response.Content.ReadFromJsonAsync<ErrorApiResponse>();
            if (content != null && content.Errors.Currencies.Contains("The selected currencies is invalid."))
                throw new CurrencyNotFoundException("Неизвестная валюта");
        }
        throw new Exception("Ошибка выполнения запроса");
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
        var response = await _currencyService.SendRequestToGetStatusAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception("Ошибка получения информации от сервера!");
        var apiStatus = await response.Content.ReadFromJsonAsync<ApiStatus>();
        if (apiStatus == null)
            throw new Exception("Ошибка преобразования данных");
        return new SettingsResult
        {
            DefaultCurrency = _apiSettingsAsOptionsMonitor.CurrentValue.DefaultCurrency,
            BaseCurrency = _apiSettingsAsOptionsMonitor.CurrentValue.BaseCurrency,
            RequestLimit = apiStatus.Quotas.Month.Total,
            RequestCount = apiStatus.Quotas.Month.Used,
            CurrencyRoundCount = _apiSettingsAsOptionsMonitor.CurrentValue.DecimalPlaces
        };
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

public record CurrencyExchangeData
{
    public Meta Meta { get; init; }
    public Dictionary<string, CurrencyData> Data { get; init; }
}

public record Meta
{
    public DateTimeOffset LastUpdatedAt { get; init; }
}

public record CurrencyData
{
    public string Code { get; init; }
    public decimal Value { get; init; }
}

public record ErrorApiResponse
{
    public string Message { get; init; }
    public ErrorApiDetails Errors { get; init; }
}

public record ErrorApiDetails
{
    public List<string> Currencies { get; init; }
}
