using Mapsui.Extensions;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mapsui.Fetcher;

public class FetchMachine
{
    readonly Channel<Func<Task>> _queue = Channel.CreateUnbounded<Func<Task>>();

    public FetchMachine(int numberOfWorkers = 4)
    {
        for (var i = 0; i < numberOfWorkers; i++)
        {
            Catch.TaskRun(() => AddConsumerAsync(_queue));
        }
    }

    public void Add(Func<Task> action)
    {
        _queue.Writer.TryWrite(action);
    }

    private static async Task AddConsumerAsync(Channel<Func<Task>> queue)
    {
        await foreach (var action in queue.Reader.ReadAllAsync())
        {
            await action();
        }
    }
}
