using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Mapsui.Fetcher
{
    class FetchWorker
    {
        private readonly IFetchDispatcher _fetchDispatcher;
        private readonly ManualResetEvent _waitHandle = new ManualResetEvent(false);
        public static long RestartCounter;

        public FetchWorker(IFetchDispatcher fetchDispatcher)
        {
            _fetchDispatcher = fetchDispatcher;
        }

        public void Start()
        {
            _waitHandle.Go();
            Debug.WriteLine("Go");
            Interlocked.Increment(ref RestartCounter);
            Task.Run(() => Fetch(_waitHandle));
        }

        public void Stop()
        {
            _waitHandle.Stop();
        }

        private void Fetch(ManualResetEvent waitHandle)
        {
            Action method = null;
            while (waitHandle.WaitOne())
            {
                if (_fetchDispatcher.TryTake(ref method))
                {
                    Debug.WriteLine("Fetch");
                    method();
                }
                else
                {
                    Debug.WriteLine("Stop");
                    waitHandle.Stop();
                }
            }
        }
    }
}
