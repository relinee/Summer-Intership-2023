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
    private readonly IOptionsMonitor<ApiSettings> _apiSettings;
    private readonly ILogger<GrpcCurrencyService> _logger;

    public GrpcCurrencyService(
        ICachedCurrencyAPI cachedCurrencyApi,
        ICurrencyAPI currencyApi,
        IOptionsMonitor<ApiSettings> apiSettings,
        ILogger<GrpcCurrencyService> logger)
    {
        _cachedCurrencyApi = cachedCurrencyApi;
        _currencyApi = currencyApi;
        _apiSettings = apiSettings;
        _logger = logger;
    }
    
    public override async Task<CurrencyResponse> GetCurrentCurrencyRate(CurrencyRequest request, ServerCallContext context)
    {
        var cancelTokenSource = new CancellationTokenSource();
        var cancellationToken = cancelTokenSource.Token;
        try
        {
            var currencyRate = await _cachedCurrencyApi.GetCurrentCurrencyAsync(
                Enum.Parse<CurrencyType>(request.CurrencyCode.ToString()),
                cancellationToken);
            return new CurrencyResponse
            {
                CurrencyCode = (CurrencyCode)currencyRate.CurrencyType,
                Value = (double) currencyRate.Value
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw new RpcException(new Status(StatusCode.Internal, "Внутренняя ошибка сервера"),
                e.Message);
        }
    }

    public override async Task<CurrencyResponse> GetCurrencyRateOnDate(CurrencyRequestWithDate request, ServerCallContext context)
    {
        // 10.08.2023 в секундах = 1691625600
        var cancelTokenSource = new CancellationTokenSource();
        var cancellationToken = cancelTokenSource.Token;
        try
        {
            var currencyRate = await _cachedCurrencyApi.GetCurrencyOnDateAsync(
                Enum.Parse<CurrencyType>(request.CurrencyCode.ToString()),
                DateOnly.FromDateTime(request.Date.ToDateTime()),
                cancellationToken);
            return new CurrencyResponse
            {
                CurrencyCode = (CurrencyCode)currencyRate.CurrencyType,
                Value = (double) currencyRate.Value
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw new RpcException(new Status(StatusCode.Internal, "Внутренняя ошибка сервера"),
                e.Message);
        }
    }

    public override async Task<SettingsResponse> GetSettings(Empty request, ServerCallContext context)
    {
        var cancelTokenSource = new CancellationTokenSource();
        var cancellationToken = cancelTokenSource.Token;
        var baseCurrencyCode = _apiSettings.CurrentValue.BaseCurrency;
        try
        {
            var convertedBaseCurrCode =
                $"{baseCurrencyCode[0]}{char.ToLower(baseCurrencyCode[1])}{char.ToLower(baseCurrencyCode[2])}";
            return new SettingsResponse
            {
                BaseCurrencyCode = Enum.Parse<CurrencyCode>(convertedBaseCurrCode),
                HasAvailableRequests = await _currencyApi.IsNewRequestsAvailable(cancellationToken)
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw new RpcException(new Status(StatusCode.Internal, "Внутренняя ошибка сервера"),
                e.Message);
        }
        
    }

    public override async Task<CurrencyResponse> GetCurrentCurrencyRateRelativeBaseCurrency(CurrencyRequestWithBaseCurrency request, ServerCallContext context)
    {
        var cancelTokenSource = new CancellationTokenSource();
        var cancellationToken = cancelTokenSource.Token;
        try
        {
            var cur = Enum.Parse<CurrencyType>(request.CurrencyCode.ToString());
            var curBase = Enum.Parse<CurrencyType>(request.BaseCurrencyCode.ToString());
            var currencyRateRelativeCacheBase = await _cachedCurrencyApi.GetCurrentCurrencyAsync(
                currencyType: cur,
                cancellationToken: cancellationToken);
            double curValue;
            if (curBase.ToString().ToUpper() == _apiSettings.CurrentValue.BaseCurrency)
            {
               curValue = (double)currencyRateRelativeCacheBase.Value;
            }
            else
            {
                var baseCurRateRelativeCacheBase = await _cachedCurrencyApi.GetCurrentCurrencyAsync(
                    currencyType: curBase,
                    cancellationToken: cancellationToken);
                curValue = (double)(currencyRateRelativeCacheBase.Value / baseCurRateRelativeCacheBase.Value);
            }
            
            return new CurrencyResponse
            {
                CurrencyCode = (CurrencyCode)currencyRateRelativeCacheBase.CurrencyType,
                Value = curValue
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw new RpcException(new Status(StatusCode.Internal, "Внутренняя ошибка сервера"),
                e.Message);
        }
    }

    public override async Task<CurrencyResponse> GetCurrencyRateOnDateRelativeBaseCurrency(CurrencyRequestWithBaseCurrencyAndDate request,
        ServerCallContext context)
    {
        // 10.08.2023 в секундах = 1691625600
        var cancelTokenSource = new CancellationTokenSource();
        var cancellationToken = cancelTokenSource.Token;
        try
        {
            var cur = Enum.Parse<CurrencyType>(request.CurrencyCode.ToString());
            var curBase = Enum.Parse<CurrencyType>(request.BaseCurrencyCode.ToString());
            var currencyRateRelativeCacheBase = await _cachedCurrencyApi.GetCurrencyOnDateAsync(
                currencyType: cur,
                date: DateOnly.FromDateTime(request.Date.ToDateTime()),
                cancellationToken: cancellationToken);
            double curValue;
            if (curBase.ToString().ToUpper() == _apiSettings.CurrentValue.BaseCurrency)
            {
                curValue = (double)currencyRateRelativeCacheBase.Value;
            }
            else
            {
                var baseCurRateRelativeCacheBase = await _cachedCurrencyApi.GetCurrencyOnDateAsync(
                    currencyType: curBase,
                    date: DateOnly.FromDateTime(request.Date.ToDateTime()),
                    cancellationToken: cancellationToken);
                curValue = (double)(currencyRateRelativeCacheBase.Value / baseCurRateRelativeCacheBase.Value);
            }
            
            return new CurrencyResponse
            {
                CurrencyCode = (CurrencyCode)currencyRateRelativeCacheBase.CurrencyType,
                Value = curValue
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw new RpcException(new Status(StatusCode.Internal, "Внутренняя ошибка сервера"),
                e.Message);
        }
    }
}