#if NETSTANDARD2_0

// ReSharper disable once CheckNamespace
namespace System.Collections.Concurrent;

public static class ConcurrentBagExtensions
{
    /// <summary>
    /// Clear Implementation for .net Standard 2.0
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    /// <param name="bag">bag</param>
    public static void Clear<T>(this ConcurrentBag<T> bag)
    {
        while (!bag.IsEmpty)
        {
            bag.TryTake(out _);
        }
    }
}
#endif
