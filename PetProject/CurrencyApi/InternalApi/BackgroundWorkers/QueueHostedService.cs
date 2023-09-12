using Fuse8_ByteMinds.SummerSchool.InternalApi.Contracts;
using Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts;
using Fuse8_ByteMinds.SummerSchool.InternalApi.DbContexts.Entities;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.BackgroundWorkers;

public class QueueHostedService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<QueueHostedService> _logger;
    private readonly IBackgroundTaskQueue _taskQueue;

    public QueueHostedService(
        IBackgroundTaskQueue taskQueue,
        IServiceProvider services,
        ILogger<QueueHostedService> logger)
    {
        _services = services;
        _logger = logger;
        _taskQueue = taskQueue;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope =_services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CurrencyRateDbContext>();
            var incompleteTasks = dbContext.CacheTasks
                .Where(t =>
                    t.Status != CacheTaskStatus.CompletedSuccessfully &&
                    t.Status != CacheTaskStatus.CompletedWithError &&
                    t.Status != CacheTaskStatus.Canceled)
                .OrderByDescending(t => t.LastUpdated)
                .ToList();
            if (incompleteTasks.Count > 0)
            {
                var firstTask = incompleteTasks.First();
                await _taskQueue.QueueAsync(new WorkItem(firstTask.Id));
                incompleteTasks.Remove(firstTask);
                foreach (var task in incompleteTasks)
                {
                    task.Status = CacheTaskStatus.Canceled;
                }

                await dbContext.SaveChangesAsync(stoppingToken);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Ошибка во время работы с бд при предварительном занесении данных в очередь задач");
        }
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await _taskQueue.DequeueAsync(stoppingToken);

            try
            {
                using var scope = _services.CreateScope();
                var worker = scope.ServiceProvider.GetRequiredService<IRecalculateCurrencyCache>();
                await worker.RecalculateCurrencyCacheToNewBaseCurrencyAsync(workItem.TaskId, stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Ошибка во время обработки {TaskId}", workItem.TaskId);
            }
        }
    }
}