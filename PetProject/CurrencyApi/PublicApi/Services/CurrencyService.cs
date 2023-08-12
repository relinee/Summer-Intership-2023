﻿using Fuse8_ByteMinds.SummerSchool.PublicApi.Contracts;
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

    public CurrencyService(GrpcCurrency.GrpcCurrencyClient grpcCurrencyClient, IOptionsMonitor<ApiSettings> apiSettings)
    {
        _grpcCurrencyClient = grpcCurrencyClient;
        _apiSettings = apiSettings;
    }

    public async Task<ExchangeRates> GetCurrencyRateAsync(CurrencyType currencyCode)
    {
        var currRequest = new CurrencyRequest{ CurrencyCode = Enum.Parse<CurrencyCode>(currencyCode.ToString())};
        var response = await _grpcCurrencyClient.GetCurrentCurrencyRateAsync(currRequest);
        var roundedValue = Math.Round(response.Value, _apiSettings.CurrentValue.DecimalPlaces);
        return new ExchangeRates
        {
            ValueCode = response.CurrencyCode.ToString(),
            CurrentValueRate = (decimal)roundedValue
        };
    }
    
    public async Task<ExchangeRatesWithDate> GetHistoricalCurrencyRateAsync(CurrencyType currencyCode, DateOnly date)
    {
        if (date == new DateOnly(1,1,1))
            throw new Exception("Ошибка преобразования даты");
        var currRequest = new CurrencyRequestWithDate
        {
            CurrencyCode = Enum.Parse<CurrencyCode>(currencyCode.ToString()),
            Date = Timestamp.FromDateTime(date.ToDateTime(TimeOnly.MaxValue).ToUniversalTime())
        };
        var response = await _grpcCurrencyClient.GetCurrencyRateOnDateAsync(currRequest);
        var roundedValue = Math.Round(response.Value, _apiSettings.CurrentValue.DecimalPlaces);
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
        return new SettingsResult
        {
            DefaultCurrency = _apiSettings.CurrentValue.DefaultCurrency,
            BaseCurrency = response.BaseCurrencyCode.ToString().ToUpper(),
            NewRequestsAvailable = response.HasAvailableRequests,
            CurrencyRoundCount = _apiSettings.CurrentValue.DecimalPlaces
        };
    }
}