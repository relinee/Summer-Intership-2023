using Fuse8_ByteMinds.SummerSchool.PublicApi.Contracts;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Controllers;

/// <summary>
/// Методы для получения курсов валют
/// </summary>
[Route("currency/favourites")]
public class CurrencyFavouriteController
{
    private readonly ICurrencyFavouriteService _currencyFavouriteService;
    private readonly CancellationTokenSource _cancellationTokenSource;
    
    public CurrencyFavouriteController(ICurrencyFavouriteService currencyFavouriteService)
    {
        _currencyFavouriteService = currencyFavouriteService;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Получить избранный курс валют по имени
    /// </summary>
    /// <param name="name">Имя</param>
    /// <response code="200">
    /// Возвращает если удалось получить избранное
    /// </response>
    /// <response code="404">
    /// Возвращает если избранного с таким именем нет
    /// </response>
    /// <response code="500">
    /// Возвращает если произошла ошибка на сервере
    /// </response>
    [HttpGet]
    [Route("{name}")]
    public async Task<ActionResult<CurrencyFavouriteModel>> GetCurrencyFavourite(string name)
    {
        return await _currencyFavouriteService.GetCurrencyFavouriteByNameAsync(name,
            _cancellationTokenSource.Token);
    }

    /// <summary>
    /// Получить все избранные курсы валют
    /// </summary>
    /// <response code="200">
    /// Возвращает если удалось получить список избранного
    /// </response>
    /// <response code="500">
    /// Возвращает если произошла ошибка на сервере
    /// </response>
    [HttpGet]
    public async Task<ActionResult<CurrencyFavouriteModel[]>> GetAllCurrencyFavourites()
        => await _currencyFavouriteService.GetAllCurrencyFavouritesAsync(_cancellationTokenSource.Token);

    /// <summary>
    /// Добавить новое избранное
    /// </summary>
    /// <param name="currencyFavouriteModel">Модель нового избранного</param>
    /// <response code="201">
    /// Возвращает если удалось добавить избранное
    /// </response>
    /// <response code="409">
    /// Возвращает если избранное с такими параметрами уже есть
    /// </response>
    /// <response code="500">
    /// Возвращает если произошла ошибка на сервере
    /// </response>
    [HttpPut]
    public async Task<ActionResult> AddCurrencyFavourite([FromBody]CurrencyFavouriteModel currencyFavouriteModel)
    {
        await _currencyFavouriteService.AddNewCurrencyFavouriteAsync(currencyFavouriteModel, _cancellationTokenSource.Token);
        return new ObjectResult("Новое избранное добавлено") {StatusCode = StatusCodes.Status201Created};
    }

    /// <summary>
    /// Обновить существующее избранное по имени
    /// </summary>
    /// <param name="name">Имя избранного</param>
    /// <param name="currencyFavouriteModel">Модель нового избранного</param>
    /// <response code="200">
    /// Возвращает если удалось изменить избранное
    /// </response>
    /// <response code="404">
    /// Возвращает если избранного с таким именем нет
    /// </response>
    /// <response code="409">
    /// Возвращает если избранное с такими параметрами уже есть
    /// </response>
    /// <response code="500">
    /// Возвращает если произошла ошибка на сервере
    /// </response>
    [HttpPost]
    [Route("{name}")]
    public async Task<ActionResult> UpdateCurrencyFavouriteAsync(
        string name,
        [FromBody] CurrencyFavouriteModel currencyFavouriteModel)
    {
        await _currencyFavouriteService.UpdateCurrencyFavouriteByNameAsync(name, currencyFavouriteModel, _cancellationTokenSource.Token);
        return new ObjectResult("Избранное обновлено") {StatusCode = StatusCodes.Status200OK};
    }
    
    /// <summary>
    /// Удаляет избранное по его имени
    /// </summary>
    /// <param name="name">Имя избранного</param>
    /// <response code="200">
    /// Возвращает если удалось удалить избранное
    /// </response>
    /// <response code="404">
    /// Возвращает если избранного с таким именем нет
    /// </response>
    /// <response code="500">
    /// Возвращает если произошла ошибка на сервере
    /// </response>
    [HttpDelete]
    [Route("{name}")]
    public async Task<ActionResult> DeleteCurrencyFavouriteAsync(string name)
    {
        await _currencyFavouriteService.DeleteCurrencyFavouriteByNameAsync(name, _cancellationTokenSource.Token);
        return new ObjectResult("Избранное удалено") {StatusCode = StatusCodes.Status200OK};
    }
}