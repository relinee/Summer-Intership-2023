using Fuse8_ByteMinds.SummerSchool.PublicApi.Contracts;
using Fuse8_ByteMinds.SummerSchool.PublicApi.DbContexts;
using Fuse8_ByteMinds.SummerSchool.PublicApi.DbContexts.Entities;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Exceptions;
using Fuse8_ByteMinds.SummerSchool.PublicApi.GrpcContracts;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Config;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Response;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Enum = System.Enum;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Services;

public class CurrencyService : ICurrencyService, ICurrencyServiceByFavouriteName
{
    private readonly GrpcCurrency.GrpcCurrencyClient _grpcCurrencyClient;
    private readonly IOptionsMonitor<ApiSettings> _apiSettings;
    private readonly CurrencyFavouritesAndSettingsDbContext _curFavAndSettDbContext;

    public CurrencyService(
        GrpcCurrency.GrpcCurrencyClient grpcCurrencyClient,
        IOptionsMonitor<ApiSettings> apiSettings,
        CurrencyFavouritesAndSettingsDbContext curFavAndSettDbContext
        )
    {
        _grpcCurrencyClient = grpcCurrencyClient;
        _apiSettings = apiSettings;
        _curFavAndSettDbContext = curFavAndSettDbContext;
    }

    public async Task<ExchangeRates> GetCurrencyRateAsync(CurrencyType currencyCode, CancellationToken cancellationToken)
    {
        var currRequest = new CurrencyRequest{ CurrencyCode = Enum.Parse<CurrencyCode>(currencyCode.ToString())};
        var response = await _grpcCurrencyClient.GetCurrentCurrencyRateAsync(currRequest, cancellationToken: cancellationToken);
        var currencySettings = GetCurrencySettingsFromDb();
        var roundedValue = Math.Round(response.Value, (int)currencySettings.CurrencyRoundCount);
        return new ExchangeRates
        {
            ValueCode = response.CurrencyCode.ToString(),
            CurrentValueRate = (decimal)roundedValue
        };
        
    }
    
    public async Task<ExchangeRatesWithDate> GetHistoricalCurrencyRateAsync(CurrencyType currencyCode, DateOnly date,
        CancellationToken cancellationToken)
    {
        if (date == new DateOnly(1,1,1))
            throw new Exception("Ошибка преобразования даты");
        var currRequest = new CurrencyRequestWithDate
        {
            CurrencyCode = Enum.Parse<CurrencyCode>(currencyCode.ToString()),
            Date = Timestamp.FromDateTime(date.ToDateTime(TimeOnly.MaxValue).ToUniversalTime())
        };
        var response = await _grpcCurrencyClient.GetCurrencyRateOnDateAsync(currRequest, cancellationToken: cancellationToken);
        var currencySettings = GetCurrencySettingsFromDb();
        var roundedValue = Math.Round(response.Value, (int)currencySettings.CurrencyRoundCount);
        return new ExchangeRatesWithDate
        {
            Date = date,
            ValueCode = response.CurrencyCode.ToString(),
            CurrentValueRate = (decimal)roundedValue
        };
    }
    
    public async Task<SettingsResult> GetSettingsAsync(CancellationToken cancellationToken)
    {
        var response = await _grpcCurrencyClient.GetSettingsAsync(new Empty(), cancellationToken: cancellationToken);
        var currencySettings = GetCurrencySettingsFromDb();
        return new SettingsResult
        {
            DefaultCurrency = _apiSettings.CurrentValue.DefaultCurrency,
            BaseCurrency = response.BaseCurrencyCode.ToString().ToUpper(),
            NewRequestsAvailable = response.HasAvailableRequests,
            CurrencyRoundCount = (int)currencySettings.CurrencyRoundCount
        };
    }
    
    public async Task<ExchangeRates> GetCurrencyRateByFavouriteNameAsync(string name, CancellationToken cancellationToken)
    {
        var currencyFavourite = await _curFavAndSettDbContext.FavouritesCurrenciesRates
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken: cancellationToken);
        if (currencyFavourite == null)
            throw new CurrencyFavouriteNotFoundException("Избранного с таким именем не найдено");
        var currRequest = new CurrencyRequestWithBaseCurrency
        {
            CurrencyCode = Enum.Parse<CurrencyCode>(ConvertStringToEnumFormat(currencyFavourite.Currency)),
            BaseCurrencyCode = Enum.Parse<CurrencyCode>(ConvertStringToEnumFormat(currencyFavourite.BaseCurrency))
        };
        var response = await _grpcCurrencyClient.GetCurrentCurrencyRateRelativeBaseCurrencyAsync(
            request: currRequest,
            cancellationToken: cancellationToken);
        var currencySettings = GetCurrencySettingsFromDb();
        var roundedValue = Math.Round(response.Value, (int)currencySettings.CurrencyRoundCount);
        return new ExchangeRates
        {
            ValueCode = response.CurrencyCode.ToString().ToUpper(),
            CurrentValueRate = (decimal)roundedValue
        };
    }

    public async Task<ExchangeRatesWithDate> GetCurrencyRateOnDateByFavouriteNameAsync(string name, DateOnly dateOnly, CancellationToken cancellationToken)
    {
        if (dateOnly == new DateOnly(1,1,1))
            throw new Exception("Ошибка преобразования даты");
        
        var currencyFavourite = await _curFavAndSettDbContext.FavouritesCurrenciesRates
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken: cancellationToken);
        if (currencyFavourite == null)
            throw new CurrencyFavouriteNotFoundException("Избранного с таким именем не найдено");
        
        var currRequest = new CurrencyRequestWithBaseCurrencyAndDate()
        {
            CurrencyCode = Enum.Parse<CurrencyCode>(ConvertStringToEnumFormat(currencyFavourite.Currency)),
            BaseCurrencyCode = Enum.Parse<CurrencyCode>(ConvertStringToEnumFormat(currencyFavourite.BaseCurrency)),
            Date = Timestamp.FromDateTime(dateOnly.ToDateTime(TimeOnly.MaxValue).ToUniversalTime())
        };

        var response = await _grpcCurrencyClient.GetCurrencyRateOnDateRelativeBaseCurrencyAsync(
            request: currRequest,
            cancellationToken: cancellationToken);
        var currencySettings = GetCurrencySettingsFromDb();
        var roundedValue = Math.Round(response.Value, (int)currencySettings.CurrencyRoundCount);
        return new ExchangeRatesWithDate
        {
            ValueCode = response.CurrencyCode.ToString().ToUpper(),
            CurrentValueRate = (decimal)roundedValue,
            Date = dateOnly
        };
    }

    private CurrencySettings GetCurrencySettingsFromDb()
    {
        var currencySettings = _curFavAndSettDbContext.Settings.FirstOrDefault();
        if (currencySettings == null)
            throw new CurrencySettingsNotFoundException("Настройки в базе данных не найдены");
        return currencySettings;
    }

    private async Task IsNewRequestsAvailableAsync(CancellationToken cancellationToken)
    {
        var response = await _grpcCurrencyClient.GetSettingsAsync(new Empty(), cancellationToken: cancellationToken);
        if (!response.HasAvailableRequests)
            throw new ApiRequestLimitException("Больше запросов нет :(");
    }

    private static string ConvertStringToEnumFormat(string str)
        => $"{str[0]}{str.ToLower()[1..3]}";
}