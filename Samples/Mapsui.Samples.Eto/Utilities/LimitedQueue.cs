
namespace Mapsui.Samples.Eto;

using System.Collections.Concurrent;
public class LimitedQueue<T> : ConcurrentQueue<T>
{
    public int Limit { get; set; }

    public LimitedQueue(int limit)
    {
        Limit = limit;
    }

    public new void Enqueue(T item)
    {
        while (Count >= Limit)
        {
            TryDequeue(out _);
        }
        base.Enqueue(item);
    }
}
