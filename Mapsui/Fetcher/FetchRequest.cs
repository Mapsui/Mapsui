using System;
using System.Threading.Tasks;
using System.Threading;

namespace Mapsui.Fetcher;

public record FetchRequest(int LayerId, Func<Task> FetchFunc)
{
    private static long _requestIdCounter = 0;
    public long RequestId { get; } = Interlocked.Increment(ref _requestIdCounter);
}
