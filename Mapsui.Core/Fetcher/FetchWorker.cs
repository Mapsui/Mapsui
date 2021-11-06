using System.Threading;
using System.Threading.Tasks;

namespace Mapsui.Fetcher
{
    public class FetchWorker // Todo: Make internal
    {
        private readonly IFetchDispatcher _fetchDispatcher;
        private CancellationTokenSource? _fetchLoopCancellationTokenSource;
        public static long RestartCounter;

        public FetchWorker(IFetchDispatcher fetchDispatcher)
        {
            _fetchDispatcher = fetchDispatcher;
        }

        public void Start()
        {
            if (_fetchLoopCancellationTokenSource == null || _fetchLoopCancellationTokenSource.IsCancellationRequested)
            {
                Interlocked.Increment(ref RestartCounter);
                _fetchLoopCancellationTokenSource = new CancellationTokenSource();
                Task.Run(() => Fetch(_fetchLoopCancellationTokenSource));
            }
        }

        public void Stop()
        {
            _fetchLoopCancellationTokenSource?.Cancel();
            _fetchLoopCancellationTokenSource = null;
        }

        private void Fetch(CancellationTokenSource? cancellationTokenSource)
        {
            while (cancellationTokenSource is { Token: { IsCancellationRequested: false } })
            {
                if (_fetchDispatcher.TryTake(out var method))
                    method();
                else
                    cancellationTokenSource.Cancel();
            }
        }
    }
}