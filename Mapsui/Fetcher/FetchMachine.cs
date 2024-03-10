using System.Collections.Generic;
using System.Threading;

namespace Mapsui.Fetcher;

public class FetchMachine // Todo: Make internal
{
    private readonly List<FetchWorker> _worker = new();

    public FetchMachine(IFetchDispatcher fetchDispatcher, int numberOfWorkers = 4)
    {
        for (var i = 0; i < numberOfWorkers; i++)
        {
            _worker.Add(new FetchWorker(fetchDispatcher));
        }
    }

    public void Start(CancellationToken? cancellationToken = null)
    {
        foreach (var worker in _worker)
        {
            worker.Start(cancellationToken);
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
