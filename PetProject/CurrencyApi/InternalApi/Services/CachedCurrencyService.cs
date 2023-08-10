using Fuse8_ByteMinds.SummerSchool.InternalApi.Contracts;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Config;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response;
using Microsoft.Extensions.Options;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services;

// TODO: сохранять кеш в папку, папку добавить в git ignore
// сделать отдельный блок в сеттинге:
// CacheSettings:{
//      Directory: "",
//      ExpirationTime: ""
//  }
internal class CachedCurrencyService : ICachedCurrencyAPI
{
    
    private readonly ICurrencyAPI _currencyApi;
    private readonly IOptionsMonitor<CurrencyCacheSettings> _cacheSettingsAsOptionsMonitor;

    public CachedCurrencyService(ICurrencyAPI currencyApi, IOptionsMonitor<CurrencyCacheSettings> cacheSettingsAsOptionsMonitor)
    {
        _currencyApi = currencyApi;
        _cacheSettingsAsOptionsMonitor = cacheSettingsAsOptionsMonitor;
    }
    
    public Task<CurrencyDTO> GetCurrentCurrencyAsync(CurrencyType currencyType, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<CurrencyDTO> GetCurrencyOnDateAsync(CurrencyType currencyType, DateOnly date, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}