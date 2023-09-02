using System.Threading.Channels;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Contracts;
using Fuse8_ByteMinds.SummerSchool.InternalApi.Models;

namespace Fuse8_ByteMinds.SummerSchool.InternalApi.BackgroundWorkers;

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<WorkItem> _queue;

    public BackgroundTaskQueue()
    {
        var options = new BoundedChannelOptions(100) {FullMode = BoundedChannelFullMode.Wait};
        _queue = Channel.CreateBounded<WorkItem>(options);
    }

    public ValueTask QueueAsync(WorkItem command)
        => _queue.Writer.WriteAsync(command);

    public ValueTask<WorkItem> DequeueAsync(CancellationToken cancellationToken)
        => _queue.Reader.ReadAsync(cancellationToken);
}