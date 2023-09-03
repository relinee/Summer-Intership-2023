using Fuse8_ByteMinds.SummerSchool.InternalApi.Contracts;
using Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts;
using Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts.Entities;
using Microsoft.EntityFrameworkCore;
using Exception = System.Exception;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Services;

public class RecalculateCurrencyCacheService : IRecalculateCurrencyCache
{
    private readonly ILogger<RecalculateCurrencyCacheService> _logger;
    private readonly CurrencyRateDbContext _currencyRateDbContext;
    
    public RecalculateCurrencyCacheService(
        ILogger<RecalculateCurrencyCacheService> logger,
        CurrencyRateDbContext currencyRateDbContext
    )
    {
        _logger = logger;
        _currencyRateDbContext = currencyRateDbContext;
    }
    
    public async Task RecalculateCurrencyCacheToNewBaseCurrencyAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var task = await _currencyRateDbContext.CacheTasks
            .FirstOrDefaultAsync(p => p.Id == taskId, cancellationToken);
        if (task == null)
        {
            _logger.LogError("Задача с id {taskId} не найдена", taskId);
            return;
        }
        if (task.Status == CacheTaskStatus.CompletedSuccessfully)
        {
            _logger.LogError("Задача с id {taskId} уже была завершена", taskId);
            return;
        }
        try
        {
            task.Status = CacheTaskStatus.InProgress;
            await _currencyRateDbContext.SaveChangesAsync(cancellationToken);

            var newBaseCur = task.NewBaseCurrency;
            var currencySettings = await _currencyRateDbContext.Settings.FirstOrDefaultAsync(cancellationToken);
            if (currencySettings == null)
            {
                _logger.LogError("Старая базовая валюта не найдена");
                return;
            }
            currencySettings.DefaultCurrency = newBaseCur;
            
            var cacheData = await _currencyRateDbContext.Currencies
                .Where(p => p.BaseCurrency != newBaseCur)
                .GroupBy(p => p.DateTime)
                .ToListAsync(cancellationToken);
            var dateSet = new HashSet<DateTimeOffset>();
            foreach (var data in cacheData)
            {
                foreach (var currRates in data)
                {
                    var oldToNewCur = currRates.Currencies.First(p => p.Code == newBaseCur);
                    var newCurr = currRates.Currencies
                        .Select(
                            i => new CurrencyRate{Code = i.Code, Value = i.Value / oldToNewCur.Value} )
                        .ToArray();
                    var newData = new CurrenciesRates
                    {
                        BaseCurrency = newBaseCur,
                        DateTime = currRates.DateTime,
                        Currencies = newCurr
                    };
                    // TODO : тут доделать
                    var isExist = _currencyRateDbContext.Currencies
                        .Any(p => p.BaseCurrency == newBaseCur && p.DateTime == currRates.DateTime);
                    if (isExist || dateSet.Contains(currRates.DateTime)) continue;
                    await _currencyRateDbContext.Currencies.AddAsync(newData, cancellationToken);
                    dateSet.Add(currRates.DateTime);
                }
            }
            task.Status = CacheTaskStatus.CompletedSuccessfully;
            await _currencyRateDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError("Произошла ошибка во время обработки {taskId} ", taskId);
            task.Status = CacheTaskStatus.CompletedWithError;
        }
    }
}