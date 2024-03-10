using System;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Logging;

namespace Mapsui.Fetcher;

public class FetchWorker(IFetchDispatcher fetchDispatcher) : IDisposable // Todo: Make internal
{
    private readonly IFetchDispatcher _fetchDispatcher = fetchDispatcher;
    private CancellationTokenSource? _fetchLoopCancellationTokenSource;
#pragma warning disable CA2211 // Non-constant fields should not be visible - This is a very special case.
    public static long RestartCounter;
#pragma warning restore CA2211

    public void Start()
    {
        if (_fetchLoopCancellationTokenSource == null || _fetchLoopCancellationTokenSource.IsCancellationRequested)
        {
            Interlocked.Increment(ref RestartCounter);
            _fetchLoopCancellationTokenSource?.Dispose();
            _fetchLoopCancellationTokenSource = new CancellationTokenSource();
            Catch.TaskRun(async () => await FetchAsync(_fetchLoopCancellationTokenSource));
        }
    }

    public void Stop()
    {
        _fetchLoopCancellationTokenSource?.Cancel();
        _fetchLoopCancellationTokenSource?.Dispose();
        _fetchLoopCancellationTokenSource = null;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _fetchLoopCancellationTokenSource?.Dispose();
            _fetchLoopCancellationTokenSource = null;
        }
    }

    private async Task FetchAsync(CancellationTokenSource? cancellationTokenSource)
    {
        try
        {
            while (cancellationTokenSource is { Token.IsCancellationRequested: false })
            {
                if (_fetchDispatcher.TryTake(out var method))
                    await method().ConfigureAwait(false);
                else
                    await cancellationTokenSource.CancelAsync();
            }
        }
        catch (ObjectDisposedException)
        {
            // Not logging on ObjectDisposedException. This happens when 
            // cancellationTokenSource.Cancel() is called.
            // Logger.Log(LogLevel.Error, e.Message, e);
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Error, e.Message, e);
        }
    }
}
