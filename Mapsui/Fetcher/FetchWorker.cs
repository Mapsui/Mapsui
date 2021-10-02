using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mapsui.Fetcher
{
    class FetchWorker
    {
        private readonly IFetchDispatcher _fetchDispatcher;
        private volatile bool _busy;
        public static long RestartCounter;

        public FetchWorker(IFetchDispatcher fetchDispatcher)
        {
            _fetchDispatcher = fetchDispatcher;
        }

        public void Start()
        {
            _busy = true;
            Interlocked.Increment(ref RestartCounter);
            Task.Run(Fetch);
        }

        public void Stop()
        {
            _busy = false;
        }

        private void Fetch()
        {
            Action method = null;
            while (_busy)
            {
                if (_fetchDispatcher.TryTake(ref method))
                {
                    method();
                }
                else
                {
                    _busy = false;
                }
            }
        }
    }
}
