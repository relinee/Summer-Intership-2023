using Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Response;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Contracts;

public interface ICurrencyFavouriteService
{
    public Task<CurrencyFavouriteModel> GetCurrencyFavouriteByNameAsync(string name, CancellationToken cancellationToken);

    public Task<CurrencyFavouriteModel[]> GetAllCurrencyFavouritesAsync(CancellationToken cancellationToken);

    public Task AddNewCurrencyFavouriteAsync(CurrencyFavouriteModel currencyFavouriteModel, CancellationToken cancellationToken);
    
    public Task UpdateCurrencyFavouriteByNameAsync(string name, CurrencyFavouriteModel newCurFavModel, CancellationToken cancellationToken);
    
    public Task DeleteCurrencyFavouriteByNameAsync(string name, CancellationToken cancellationToken);
}