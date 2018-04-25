using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Mapsui.Tests")]
namespace Mapsui.Fetcher
{
    class FetchMachine
    {
        private readonly List<FetchWorker> _worker = new List<FetchWorker>();
        
        public FetchMachine(IFetchDispatcher fetchDispatcher, int numberOfWorkers = 4)
        {
            for (int i = 0; i < numberOfWorkers; i++)
            {
                _worker.Add(new FetchWorker(fetchDispatcher));
            }
        }

        public void Start()
        {
            foreach (var worker in _worker)
            {
                worker.Start();
            }
        }
        
        public void Stop()
        {
            foreach (var worker in _worker)
            {
                worker.Stop();
            }
        }
    }
}
