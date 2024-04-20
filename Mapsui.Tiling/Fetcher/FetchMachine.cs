using Mapsui.Tiling.Fetcher;
using System.Collections.Generic;

namespace Mapsui.Fetcher;

public class FetchMachine
{
    private readonly List<FetchWorker> _worker = [];

    public FetchMachine(TileFetchDispatcher fetchDispatcher, int numberOfWorkers = 4)
    {
        for (var i = 0; i < numberOfWorkers; i++)
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
