using Mapsui.Extensions;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mapsui.Fetcher;

public class FeatureFetchMachine
{
    readonly Channel<Func<Task>> _queue = Channel.CreateUnbounded<Func<Task>>();

    public FeatureFetchMachine(int numberOfWorkers = 4)
    {
        for (var i = 0; i < numberOfWorkers; i++)
        {
            Catch.TaskRun(() => AddConsumerAsync(_queue));
        }
    }

    private async Task AddConsumerAsync(Channel<Func<Task>> queue)
    {
        await foreach (var action in queue.Reader.ReadAllAsync())
        {
            await action();
        }
    }

    public void Start(Func<Task> action)
    {
        _queue.Writer.TryWrite(action);
    }

    public void Stop()
    {
        // Todo: implement
    }
}
