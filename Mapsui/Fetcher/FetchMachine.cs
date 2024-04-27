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
            _ = AddConsumerAsync(_queue);
    }

    public void Start(Func<Task> action) => _queue.Writer.TryWrite(action);

    public void Stop() { }

    private static async Task AddConsumerAsync(Channel<Func<Task>> queue)
    {
        await foreach (var action in queue.Reader.ReadAllAsync().ConfigureAwait(false))
            await action().ConfigureAwait(false);
    }
}
