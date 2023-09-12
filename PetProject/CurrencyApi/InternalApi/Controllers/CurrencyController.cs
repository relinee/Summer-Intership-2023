using Fuse8_ByteMinds.SummerSchool.InternalApi.Contracts;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Controllers;

/// <summary>
/// Методы для получения курсов валют
/// </summary>
[Route("currency")]
public class CurrencyController : Controller
{
    private readonly ICurrencyRestService _currencyRestService;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public CurrencyController(ICurrencyRestService currencyRestService)
    {
        _currencyRestService = currencyRestService;
        _cancellationTokenSource = new CancellationTokenSource();
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
        var cancellationToken = _cancellationTokenSource.Token;
        var currencyRate = await _currencyRestService.GetCurrentCurrencyRateAsync(
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
        var cancellationToken = _cancellationTokenSource.Token;
        var currencyRate = await _currencyRestService.GetCurrencyRateOnDateAsync(
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
    public async Task<ActionResult<CurrencySettings>> GetCurrencySettings()
    {
        var cancellationToken = _cancellationTokenSource.Token;
        return await _currencyRestService.GetSettingsAsync(cancellationToken);
    }

    /// <summary>
    /// Проверка работоспособности сервиса
    /// </summary>
    /// <response code="200">
    /// Возвращает если сервис работает
    /// </response>
    /// <response code="500">
    /// Возвращает если произошла ошибка на сервере
    /// </response>
    [HttpGet("health")]
    public string Check() => "Healthy";


    /// <summary>
    /// Создать задачу для пересчета курсов на новую базовую валюту
    /// </summary>
    /// <param name="newBaseCurrency">Новая базовая валюта</param>
    /// <response code="202">
    /// Возвращает вместе с Guid задачи, если ее удалось создать
    /// </response>
    /// <response code="500">
    /// Возвращает если произошла ошибка на сервере
    /// </response>
    [HttpPost]
    [Route("cache/recalculate/")]
    public async Task<IActionResult> RecalculateCache([FromBody]CurrencyType newBaseCurrency)
    {
        var cancellationToken = _cancellationTokenSource.Token;
        var guid = await _currencyRestService.CreateTaskToRecalculateCacheToNewBaseCurrencyAsync(newBaseCurrency, cancellationToken);
        return Accepted(guid);
    }
}