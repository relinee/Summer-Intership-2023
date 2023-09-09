using Fuse8_ByteMinds.SummerSchool.PublicApi.Contracts;
using Fuse8_ByteMinds.SummerSchool.PublicApi.DbContexts;
using Fuse8_ByteMinds.SummerSchool.PublicApi.DbContexts.Entities;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Exceptions;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Response;
using Microsoft.EntityFrameworkCore;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Services;

public class CurrencyFavouriteService : ICurrencyFavouriteService
{
    private readonly CurrencyFavouritesAndSettingsDbContext _curFavAndSettDbContext;

    public CurrencyFavouriteService(
        CurrencyFavouritesAndSettingsDbContext curFavAndSettDbContext
    )
    {
        _curFavAndSettDbContext = curFavAndSettDbContext;
    }
    
    public async Task<CurrencyFavouriteModel> GetCurrencyFavouriteByNameAsync(string name, CancellationToken cancellationToken)
    {
        var currencyFavourite = await _curFavAndSettDbContext.FavouritesCurrenciesRates
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
        
        if (currencyFavourite == null)
        {
            throw new CurrencyFavouriteNotFoundException("Избранного с таким именем нет");
        }

        return ConvertFavToFavModel(currencyFavourite);
    }

    public async Task<CurrencyFavouriteModel[]> GetAllCurrencyFavouritesAsync(CancellationToken cancellationToken)
    {
        var currencyFavourite = await _curFavAndSettDbContext.FavouritesCurrenciesRates
            .ToArrayAsync(cancellationToken);
        
        return Array.ConvertAll(currencyFavourite, ConvertFavToFavModel);
    }

    public async Task AddNewCurrencyFavouriteAsync(CurrencyFavouriteModel currencyFavouriteModel, CancellationToken cancellationToken)
    {
        if (IsAlreadyNameInDb(currencyFavouriteModel.Name) ||
            IsAlreadyCurAndCurBaseInDb(
                currencyFavouriteModel.Currency.ToString().ToUpper(),
                currencyFavouriteModel.BaseCurrency.ToString().ToUpper()))
        {
            throw new CurrencyFavouriteIsAlreadyExistError("Избранное с таким именем или параметрами уже существует");
        }
        
        await _curFavAndSettDbContext.FavouritesCurrenciesRates.AddAsync(
            new CurrencyFavourite
            {
                BaseCurrency = currencyFavouriteModel.BaseCurrency.ToString().ToUpper(),
                Currency = currencyFavouriteModel.Currency.ToString().ToUpper(),
                Name = currencyFavouriteModel.Name
            },
            cancellationToken);
        await _curFavAndSettDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateCurrencyFavouriteByNameAsync(string name, CurrencyFavouriteModel newCurFavModel, CancellationToken cancellationToken)
    {
        var currCurrencyFavourite = await _curFavAndSettDbContext.FavouritesCurrenciesRates
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
        if (currCurrencyFavourite == null)
        {
            throw new CurrencyFavouriteNotFoundException("Избранного с таким именем нет");
        }

        var newCurString = newCurFavModel.Currency.ToString().ToUpper();
        var newBaseCurString = newCurFavModel.BaseCurrency.ToString().ToUpper();
        if ((IsAlreadyNameInDb(newCurFavModel.Name) && name != newCurFavModel.Name) ||
            (IsAlreadyCurAndCurBaseInDb(newCurString, newBaseCurString) &&
             GetNameByCurAndCurBaseAsync(newCurString, newBaseCurString) != name))
        {
            throw new CurrencyFavouriteIsAlreadyExistError("Избранное с таким именем или параметрами уже существует");
        }
        
        currCurrencyFavourite.Name = newCurFavModel.Name;
        currCurrencyFavourite.Currency = newCurFavModel.Currency.ToString().ToUpper();
        currCurrencyFavourite.BaseCurrency = newCurFavModel.BaseCurrency.ToString().ToUpper();
        await _curFavAndSettDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteCurrencyFavouriteByNameAsync(string name, CancellationToken cancellationToken)
    {
        var currCurrencyFavourite = await _curFavAndSettDbContext.FavouritesCurrenciesRates
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
        if (currCurrencyFavourite == null)
        {
            throw new CurrencyFavouriteNotFoundException("Избранного с таким именем нет");
        }
        _curFavAndSettDbContext.FavouritesCurrenciesRates.Remove(currCurrencyFavourite);
        await _curFavAndSettDbContext.SaveChangesAsync(cancellationToken);
    }

    private static CurrencyFavouriteModel ConvertFavToFavModel(CurrencyFavourite currencyFavourite)
    {
        var currency = Enum.Parse<CurrencyType>(
            currencyFavourite.Currency, ignoreCase: true);
        var baseCurrency = Enum.Parse<CurrencyType>(
            currencyFavourite.BaseCurrency, ignoreCase: true);
        return new CurrencyFavouriteModel(currencyFavourite.Name, currency, baseCurrency);
    }

    private bool IsAlreadyNameInDb(string name)
    {
        var currCurrencyFavourite = _curFavAndSettDbContext.FavouritesCurrenciesRates
            .FirstOrDefault(c => c.Name == name);
        return currCurrencyFavourite != null;
    }
    
    private bool IsAlreadyCurAndCurBaseInDb(string currency, string currencyBase)
        => _curFavAndSettDbContext.FavouritesCurrenciesRates
            .Any(c => c.Currency == currency && c.BaseCurrency == currencyBase);
    

    private string? GetNameByCurAndCurBaseAsync(string currency, string currencyBase)
    {
        var currCurrencyFavourite = _curFavAndSettDbContext.FavouritesCurrenciesRates
            .FirstOrDefault(c =>
                c.Currency == currency && c.BaseCurrency == currencyBase);
        return currCurrencyFavourite?.Name;
    }
}