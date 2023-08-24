using System.Data;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Contracts;
using Fuse8_ByteMinds.SummerSchool.PublicApi.DbContexts;
using Fuse8_ByteMinds.SummerSchool.PublicApi.DbContexts.Entities;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Exceptions;
using Fuse8_ByteMinds.SummerSchool.PublicApi.GrpcContracts;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Config;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models.Response;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using Enum = System.Enum;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Services;

public class CurrencyService : ICurrencyService
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

    public async Task<ExchangeRates> GetCurrencyRateAsync(CurrencyType currencyCode)
    {
        await IsNewRequestsAvailable();
        
        var currRequest = new CurrencyRequest{ CurrencyCode = Enum.Parse<CurrencyCode>(currencyCode.ToString())};
        var response = await _grpcCurrencyClient.GetCurrentCurrencyRateAsync(currRequest);
        var currencySettings = GetCurrencySettingsFromDb();
        var roundedValue = Math.Round(response.Value, (int)currencySettings.CurrencyRoundCount);
        return new ExchangeRates
        {
            ValueCode = response.CurrencyCode.ToString(),
            CurrentValueRate = (decimal)roundedValue
        };
    }
    
    public async Task<ExchangeRatesWithDate> GetHistoricalCurrencyRateAsync(CurrencyType currencyCode, DateOnly date)
    {
        await IsNewRequestsAvailable();
        
        if (date == new DateOnly(1,1,1))
            throw new Exception("Ошибка преобразования даты");
        var currRequest = new CurrencyRequestWithDate
        {
            CurrencyCode = Enum.Parse<CurrencyCode>(currencyCode.ToString()),
            Date = Timestamp.FromDateTime(date.ToDateTime(TimeOnly.MaxValue).ToUniversalTime())
        };
        var response = await _grpcCurrencyClient.GetCurrencyRateOnDateAsync(currRequest);
        var currencySettings = GetCurrencySettingsFromDb();
        var roundedValue = Math.Round(response.Value, (int)currencySettings.CurrencyRoundCount);
        return new ExchangeRatesWithDate
        {
            Date = date,
            ValueCode = response.CurrencyCode.ToString(),
            CurrentValueRate = (decimal)roundedValue
        };
    }
    
    public async Task<SettingsResult> GetSettingsAsync()
    {
        var response = await _grpcCurrencyClient.GetSettingsAsync(new Empty());
        var currencySettings = GetCurrencySettingsFromDb();
        return new SettingsResult
        {
            DefaultCurrency = _apiSettings.CurrentValue.DefaultCurrency,
            BaseCurrency = response.BaseCurrencyCode.ToString().ToUpper(),
            NewRequestsAvailable = response.HasAvailableRequests,
            CurrencyRoundCount = (int)currencySettings.CurrencyRoundCount
        };
    }

    private CurrencySettings GetCurrencySettingsFromDb()
    {
        var currencySettings = _curFavAndSettDbContext.Settings.FirstOrDefault();
        if (currencySettings == null)
            throw new CurrencySettingsNotFoundException("Настройки в базе данных не найдены");
        return currencySettings;
    }

    private async Task IsNewRequestsAvailable()
    {
        var response = await _grpcCurrencyClient.GetSettingsAsync(new Empty());
        if (!response.HasAvailableRequests)
            throw new ApiRequestLimitException("Больше запросов нет :(");
    }

}