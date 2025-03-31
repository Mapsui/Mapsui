using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Mapsui.Extensions;

public static class ConcurrentQueueExtensions
{
    // This method was added to make the api more compatible with the previous 
    // List based Widgets library
    public static void Add<T>(this ConcurrentQueue<T> queue, T item)
    {
        queue.Enqueue(item);
    }

    public static void Clear<T>(this ConcurrentQueue<T> queue)
    {
        while (queue.TryDequeue(out _)) { }
    }

    public static void AddRange<T>(this ConcurrentQueue<T> queue, IEnumerable<T> itemsToAdd)
    {
        foreach (var layer in itemsToAdd)
        {
            queue.Enqueue(layer);
        }
    }
}
