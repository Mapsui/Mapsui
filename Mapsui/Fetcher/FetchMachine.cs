using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mapsui.Fetcher;

public class FetchMachine
{
    readonly Channel<Func<Task>> _channel = Channel.CreateUnbounded<Func<Task>>(new UnboundedChannelOptions { AllowSynchronousContinuations = true, SingleReader = false });

    public int NumberOfWorkers { get; }

    public FetchMachine(int numberOfWorkers = 4)
    {
        NumberOfWorkers = 4;
        for (var i = 0; i < numberOfWorkers; i++)
            _ = AddConsumerAsync(_channel);
    }

    public void Enqueue(Func<Task> action) => _channel.Writer.TryWrite(action);

    public void Stop() { }


    private static async Task AddConsumerAsync(Channel<Func<Task>> queue)
    {
        await foreach (var action in queue.Reader.ReadAllAsync().ConfigureAwait(false))
            await action().ConfigureAwait(false);
    }
}
