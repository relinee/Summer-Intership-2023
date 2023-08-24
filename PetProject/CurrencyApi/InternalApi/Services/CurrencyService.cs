using System.Net;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Contracts;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Exceptions;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Config;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response.ExternalAPI;
using Microsoft.Extensions.Options;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services;

public class CurrencyService : ICurrencyAPI
{
    private readonly HttpClient _httpClient;
    private readonly IOptionsMonitor<ApiSettings> _apiSettings;

    public CurrencyService(HttpClient httpClient, IOptionsMonitor<ApiSettings> apiSettings)
    {
        _httpClient = httpClient;
        _apiSettings = apiSettings;
    }

    public async Task<Currency[]> GetAllCurrentCurrenciesAsync(string baseCurrency, CancellationToken cancellationToken)
    {
        // TODO : добавить проверку на количество запросов
        
        var response = await SendRequestToGetCurrencyRateAsync(baseCurrency, "");
        if (response.IsSuccessStatusCode)
        {
            var latestCurrencyExchangeData = await response.Content.ReadFromJsonAsync<CurrencyExchangeData>();
            if (latestCurrencyExchangeData?.Data == null || latestCurrencyExchangeData.Meta == null)
                throw new Exception("Ошибка преобразования данных");
            var currenciesList = new List<Currency>(); 
            foreach (var currCurrency in latestCurrencyExchangeData.Data)
            {
                currenciesList.Add(new Currency (currCurrency.Value.Code, currCurrency.Value.Value) );
            }
            return currenciesList.ToArray();
        }

        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            var content = await response.Content.ReadFromJsonAsync<ErrorApiResponse>();
            if (content != null && content.Errors.Currencies.Contains("The selected currencies is invalid."))
                throw new CurrencyNotFoundException("Неизвестная валюта");
        }
        throw new Exception("Ошибка выполнения запроса");
    }

    public async Task<CurrenciesOnDate> GetAllCurrenciesOnDateAsync(string baseCurrency, DateOnly date, CancellationToken cancellationToken)
    {
        if (date == new DateOnly(1,1,1))
            throw new Exception("Ошибка преобразования даты");
        var response = await SendRequestToGetHistoricalCurrencyRateAsync(baseCurrency, "", date);
        // var requestString = _apiSettingsAsOptionsMonitor.CurrentValue.BaseUrl +
        //                     $"/historical?date={date.ToString("yyyy-MM-dd")}&base_currency={baseCurrency}";
        // var response = await SendRequestWithPath(requestString);
        if (response.IsSuccessStatusCode)
        {
            var historicalCurrencyExchangeData = await response.Content.ReadFromJsonAsync<CurrencyExchangeData>();
            if (historicalCurrencyExchangeData?.Data == null || historicalCurrencyExchangeData.Meta == null)
                throw new Exception("Ошибка преобразования данных");
            var currenciesList = new List<Currency>(); 
            foreach (var currCurrency in historicalCurrencyExchangeData.Data)
            {
                currenciesList.Add(new Currency(currCurrency.Value.Code, currCurrency.Value.Value) );
            }

            return new CurrenciesOnDate(
                historicalCurrencyExchangeData.Meta.LastUpdatedAt.UtcDateTime,
                currenciesList.ToArray());
        }

        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
        {
            var content = await response.Content.ReadFromJsonAsync<ErrorApiResponse>();
            if (content != null && content.Errors.Currencies.Contains("The selected currencies is invalid."))
                throw new CurrencyNotFoundException("Неизвестная валюта");
        }
        throw new Exception("Ошибка выполнения запроса");
    }
    
    public async Task<bool> IsNewRequestsAvailable(CancellationToken cancellationToken)
    {
        var response = await SendRequestToGetStatusAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception("Ошибка получения количества доступных запросов!");
        
        var apiStatus = await response.Content.ReadFromJsonAsync<ApiStatus>();
        if (apiStatus == null)
            throw new Exception("Ошибка преобразования данных");
        return apiStatus.Quotas.Month.Used < apiStatus.Quotas.Month.Total;
    }

    private Task<HttpResponseMessage> SendRequestToGetStatusAsync()
    {
        return SendRequestWithPath(_apiSettings.CurrentValue.BaseUrl + "/status");
    }
    
    private Task<HttpResponseMessage> SendRequestToGetCurrencyRateAsync(string baseCurrencyCode, string defaultCurrencyCode)
    {
        return SendRequestWithPath(_apiSettings.CurrentValue.BaseUrl + $"/latest?currencies={defaultCurrencyCode}&base_currency={baseCurrencyCode}");
    }
    
    private Task<HttpResponseMessage> SendRequestToGetHistoricalCurrencyRateAsync(string baseCurrencyCode, string defaultCurrencyCode, DateOnly date)
    {
        return SendRequestWithPath(_apiSettings.CurrentValue.BaseUrl + $"/historical?currencies={defaultCurrencyCode}&date={date.ToString("yyyy-MM-dd")}&base_currency={baseCurrencyCode}");
    }

    private async Task<HttpResponseMessage> SendRequestWithPath(string path)
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("apikey", _apiSettings.CurrentValue.ApiKey);
        return await _httpClient.GetAsync(path);
    }
}