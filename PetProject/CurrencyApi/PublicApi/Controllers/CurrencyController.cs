using Fuse8_ByteMinds.SummerSchool.PublicApi.Contracts;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Config;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Controllers;

/// <summary>
/// Методы для получения курсов валют
/// </summary>
[Route("currency")]
public class CurrencyController : ControllerBase
{
    private readonly IOptionsMonitor<ApiSettings> _apiSettings;
    private readonly ICurrencyService _currencyService;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ICurrencyServiceByFavouriteName _currencyServiceByFavouriteName;
    
    public CurrencyController(
        IOptionsMonitor<ApiSettings> apiSettings,
        ICurrencyService currencyService,
        ICurrencyServiceByFavouriteName currencyServiceByFavouriteName)
    {
        _apiSettings = apiSettings;
        _currencyService = currencyService;
        _cancellationTokenSource = new CancellationTokenSource();
        _currencyServiceByFavouriteName = currencyServiceByFavouriteName;
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
    public async Task<ActionResult<ExchangeRates>> GetDefaultCurrencyRate()
    {
        var defaultCurrency = _apiSettings.CurrentValue.DefaultCurrency;
        var correctedDefaultCurrency =
            Enum.Parse<CurrencyType>($"{defaultCurrency[0]}{defaultCurrency[1..].ToLower()}");
        return await _currencyService.GetCurrencyRateAsync(correctedDefaultCurrency, _cancellationTokenSource.Token);
    }


    /// <summary>
    /// Получить курс для валюты по выбору 
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
    [HttpGet]
    [Route("{currencyCode}")]
    public async Task<ActionResult<ExchangeRates>> GetNotDefaultCurrencyRate(CurrencyType currencyCode)
    {
        return await _currencyService.GetCurrencyRateAsync(currencyCode, _cancellationTokenSource.Token);
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
    public async Task<ActionResult<ExchangeRatesWithDate>> GetHistoricalCurrencyRate(CurrencyType currencyCode, DateOnly date)
    {
        return await _currencyService.GetHistoricalCurrencyRateAsync(currencyCode, date, _cancellationTokenSource.Token);
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
    public async Task<ActionResult<SettingsResult>> GetSettings()
    {
        return await _currencyService.GetSettingsAsync(_cancellationTokenSource.Token);
    }

    /// <summary>
    /// Получить курс валют по названию избранного
    /// </summary>
    /// <param name="name">Название избранного</param>
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
    /// <response code="404">
    /// Возвращает если избранного с таким именем нет
    /// </response>
    [HttpGet]
    [Route("favourite/{name}")]
    public async Task<ActionResult<ExchangeRates>> GetNotCurrencyRateByFavouriteName(string name)
    {
        return await _currencyServiceByFavouriteName.GetCurrencyRateByFavouriteNameAsync(name, _cancellationTokenSource.Token);
    }

    /// <summary>
    /// Получить курс валют по названию избранного на дату
    /// </summary>
    /// <param name="name">Название избранного</param>
    /// <param name="date">Дата в формате YYYY-MM-DD</param>
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
    /// <response code="404">
    /// Возвращает если избранного с таким именем нет
    /// </response>
    [HttpGet]
    [Route("favourite/{name}/{date}")]
    public async Task<ActionResult<ExchangeRatesWithDate>> GetNotCurrencyRateByFavouriteNameOnDate(string name, DateOnly date)
    {
        return await _currencyServiceByFavouriteName.GetCurrencyRateOnDateByFavouriteNameAsync(name, date,
            _cancellationTokenSource.Token);
    }
}
