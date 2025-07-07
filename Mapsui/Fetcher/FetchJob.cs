using System;
using System.Threading.Tasks;
using System.Threading;

namespace Mapsui.Fetcher;

public record FetchJob(int LayerId, Func<Task> FetchFunc)
{
    private static long _requestIdCounter = 0;
    public long JobId { get; } = Interlocked.Increment(ref _requestIdCounter);
}
