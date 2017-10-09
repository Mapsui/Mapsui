using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mapsui.Fetcher
{
    class FetchWorker
    {
        private readonly IFetchDispatcher _fetchDispatcher;
        private CancellationTokenSource _fetchLoopCancellationTokenSource;
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
        }

        private void Fetch(CancellationTokenSource cancellationTokenSource)
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                var fetchOrder = _fetchDispatcher.TakeFetchOrder();
                if (fetchOrder == null)
                {
                    cancellationTokenSource.Cancel();
                }
                else
                {
                    fetchOrder.ExecuteOrder();
                }
            }
        }
    }
}
