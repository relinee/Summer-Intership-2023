using Fuse8_ByteMinds.SummerSchool.InternalApi.Contracts;
using Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts;
using Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts.Entities;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Exceptions;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response;
using CurrencySettings = Fuse8_ByteMinds.SummerSchool.InternalApi.Models.Response.CurrencySettings;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services;

public class CurrencyRestService : ICurrencyRestService
{
    
    private readonly ICachedCurrencyAPI _cachedCurrencyApi;
    private readonly ICurrencyAPI _currencyApi;
    private readonly CurrencyRateDbContext _currencyRateDbContext;
    private readonly IBackgroundTaskQueue _taskQueue;

    public CurrencyRestService(
        ICachedCurrencyAPI cachedCurrencyApi,
        ICurrencyAPI currencyApi,
        CurrencyRateDbContext currencyRateDbContext,
        IBackgroundTaskQueue taskQueue)
    {
        _cachedCurrencyApi = cachedCurrencyApi;
        _currencyApi = currencyApi;
        _currencyRateDbContext = currencyRateDbContext;
        _taskQueue = taskQueue;
    }
    
    public async Task<CurrencyDTO> GetCurrentCurrencyRateAsync(CurrencyType currencyType, CancellationToken cancellationToken)
    {
        var currencyRate = await _cachedCurrencyApi.GetCurrentCurrencyAsync(
            currencyType: currencyType,
            cancellationToken: cancellationToken);
        return currencyRate;
    }
    
    public async Task<CurrencyDTO> GetCurrencyRateOnDateAsync(CurrencyType currencyType, DateOnly date, CancellationToken cancellationToken)
    {
        var currencyRate = await _cachedCurrencyApi.GetCurrencyOnDateAsync(
            currencyType: currencyType,
            date: date,
            cancellationToken: cancellationToken);
        return currencyRate;
    }

    public async Task<CurrencySettings> GetSettingsAsync(CancellationToken cancellationToken)
    {
        var baseCurrencyCode = GetBaseCurrencyFromDb();
        var convertedBaseCurrCode =
            $"{baseCurrencyCode[0]}{char.ToLower(baseCurrencyCode[1])}{char.ToLower(baseCurrencyCode[2])}";
        var isNewRequestAvailable = await _currencyApi.IsNewRequestsAvailable(cancellationToken);
        return new CurrencySettings(Enum.Parse<CurrencyType>(convertedBaseCurrCode), isNewRequestAvailable);
    }

    public async Task<Guid> CreateTaskToRecalculateCacheToNewBaseCurrencyAsync(CurrencyType newBaseCurrency,
        CancellationToken cancellationToken)
    {
        var cacheTask = new CacheTask
        {
            NewBaseCurrency = newBaseCurrency.ToString().ToUpper(),
            LastUpdated = DateTimeOffset.UtcNow,
            Status = CacheTaskStatus.Created
        };
        _currencyRateDbContext.CacheTasks.Add(cacheTask);
        await _currencyRateDbContext.SaveChangesAsync(cancellationToken);
        await _taskQueue.QueueAsync(new WorkItem(cacheTask.Id));
        return cacheTask.Id;
    }

    private string GetBaseCurrencyFromDb()
    {
        var baseCurrency = _currencyRateDbContext.Settings.FirstOrDefault();
        if (baseCurrency == null)
        {
            throw new CurrencySettingsNotFoundException("Базовой валюты не найдено");
        }

        return baseCurrency.DefaultCurrency;
    }
}