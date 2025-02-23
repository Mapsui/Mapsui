using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Mapsui.Fetcher;

public sealed class FetchMachine : IDisposable
{
    private readonly Channel<Func<CancellationToken, Task>> _queue = Channel.CreateUnbounded<Func<CancellationToken, Task>>();
    private CancellationTokenSource _cancellationTokenSource;

    public FetchMachine(int numberOfWorkers = 4)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        for (var i = 0; i < numberOfWorkers; i++)
            _ = AddConsumerAsync(_queue);
    }

    public void Start(Func<CancellationToken, Task> action) => _queue.Writer.TryWrite(action);

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    private async Task AddConsumerAsync(Channel<Func<CancellationToken, Task>> queue)
    {
        await foreach (var action in queue.Reader.ReadAllAsync().ConfigureAwait(false))
            await action(_cancellationTokenSource.Token).ConfigureAwait(false);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}
