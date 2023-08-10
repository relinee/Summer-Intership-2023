using Fuse8_ByteMinds.SummerSchool.InternalApi.Contracts;
using Fuse8_ByteMinds.SummerSchool.InternalApi.GrpcContracts;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Config;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Options;
using Enum = System.Enum;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services;

public class GrpcCurrencyService : GrpcCurrency.GrpcCurrencyBase
{
    private readonly ICachedCurrencyAPI _cachedCurrencyApi;
    private readonly ICurrencyAPI _currencyApi;
    private readonly IOptionsMonitor<ApiSettings> _apiSettingsAsOptionsMonitor;

    public GrpcCurrencyService(
        ICachedCurrencyAPI cachedCurrencyApi,
        ICurrencyAPI currencyApi,
        IOptionsMonitor<ApiSettings> apiSettingsAsOptionsMonitor)
    {
        _cachedCurrencyApi = cachedCurrencyApi;
        _currencyApi = currencyApi;
        _apiSettingsAsOptionsMonitor = apiSettingsAsOptionsMonitor;
    }
    
    public override async Task<CurrencyResponse> GetCurrentCurrencyRate(CurrencyRequest request, ServerCallContext context)
    {
        var cancellationToken = new CancellationToken();
        var currencyRate = await _cachedCurrencyApi.GetCurrentCurrencyAsync(
            Enum.Parse<CurrencyType>( nameof(request.CurrencyCode)),
            cancellationToken);
        return new CurrencyResponse
        {
            CurrencyCode = (CurrencyCode)currencyRate.CurrencyType,
            Value = (double) currencyRate.Value
        };
    }

    public override async Task<CurrencyResponse> GetCurrencyRateOnDate(CurrencyRequestWithDate request, ServerCallContext context)
    {
        var cancellationToken = new CancellationToken();
        var currencyRate = await _cachedCurrencyApi.GetCurrencyOnDateAsync(
            Enum.Parse<CurrencyType>(  nameof(request.CurrencyCode)),
            DateOnly.FromDateTime(request.Date.ToDateTime()),
            cancellationToken);
        return new CurrencyResponse
        {
            CurrencyCode = (CurrencyCode)currencyRate.CurrencyType,
            Value = (double) currencyRate.Value
        };
    }

    public override async Task<SettingsResponse> GetSettings(Empty request, ServerCallContext context)
    {
        var cancellationToken = new CancellationToken();
        var baseCurrencyCode = _apiSettingsAsOptionsMonitor.CurrentValue.BaseCurrency;
        var convertedBaseCurrCode =
            $"{baseCurrencyCode[0]}{char.ToLower(baseCurrencyCode[1])}{char.ToLower(baseCurrencyCode[2])}";
        return new SettingsResponse
        {
            BaseCurrencyCode = Enum.Parse<CurrencyCode>(convertedBaseCurrCode),
            HasAvailableRequests = await _currencyApi.IsNewRequestsAvailable(cancellationToken)
        };
    }

}