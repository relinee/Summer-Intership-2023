using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.Contracts;

public interface IBackgroundTaskQueue
{
    ValueTask QueueAsync(WorkItem command);

    ValueTask<WorkItem> DequeueAsync(CancellationToken cancellationToken);
}