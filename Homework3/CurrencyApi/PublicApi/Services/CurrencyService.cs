﻿using System.Net;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Controllers;
using Fuse8_ByteMinds.SummerSchool.PublicApi.Models;
using Microsoft.Extensions.Options;

namespace Fuse8_ByteMinds.SummerSchool.PublicApi.Services;

public class CurrencyService : ICurrencyService
{
    private readonly HttpClient _httpClient;
    private readonly IOptionsMonitor<ApiSettings> _apiSettingsAsOptionsMonitor;

    public CurrencyService(HttpClient httpClient, IOptionsMonitor<ApiSettings> apiSettingsAsOptionsMonitor)
    {
        _httpClient = httpClient;
        _apiSettingsAsOptionsMonitor = apiSettingsAsOptionsMonitor;
    }

    public async Task<LatestExchangeRates> GetCurrencyRateAsync(string currencyCode)
    {
        var response = await SendRequestToGetCurrencyRateAsync(_apiSettingsAsOptionsMonitor.CurrentValue.BaseCurrency, currencyCode);
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
    
    public async Task<HistoricalExchangeRates> GetHistoricalCurrencyRateAsync(string currencyCode, DateOnly date)
    {
        if (date == new DateOnly(1,1,1))
            throw new Exception("Ошибка преобразования даты");
        var response = await SendRequestToGetHistoricalCurrencyRateAsync(_apiSettingsAsOptionsMonitor.CurrentValue.BaseCurrency, currencyCode, date);
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
    
    public async Task<SettingsResult> GetSettingsAsync()
    {
        var response = await SendRequestToGetStatusAsync();
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

    public async Task<ApiStatus> CheckApiStatusAsync()
    {
        var response = await SendRequestToGetStatusAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception("Ошибка получения количества доступных запросов!");
        
        var apiStatus = await response.Content.ReadFromJsonAsync<ApiStatus>();
        if (apiStatus == null)
            throw new Exception("Ошибка преобразования данных");
        return apiStatus;
    }

    private Task<HttpResponseMessage> SendRequestToGetStatusAsync()
    {
        return SendRequestWithPath(_apiSettingsAsOptionsMonitor.CurrentValue.BaseUrl + "/status");
    }
    
    private Task<HttpResponseMessage> SendRequestToGetCurrencyRateAsync(string baseCurrencyCode, string defaultCurrencyCode)
    {
        return SendRequestWithPath(_apiSettingsAsOptionsMonitor.CurrentValue.BaseUrl + $"/latest?currencies={defaultCurrencyCode}&base_currency={baseCurrencyCode}");
    }
    
    private Task<HttpResponseMessage> SendRequestToGetHistoricalCurrencyRateAsync(string baseCurrencyCode, string defaultCurrencyCode, DateOnly date)
    {
        return SendRequestWithPath(_apiSettingsAsOptionsMonitor.CurrentValue.BaseUrl + $"/historical?currencies={defaultCurrencyCode}&date={date.ToString("yyyy-MM-dd")}&base_currency={baseCurrencyCode}");
    }

    private async Task<HttpResponseMessage> SendRequestWithPath(string path)
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("apikey", _apiSettingsAsOptionsMonitor.CurrentValue.ApiKey);
        return await _httpClient.GetAsync(path);
    }
    
    
}