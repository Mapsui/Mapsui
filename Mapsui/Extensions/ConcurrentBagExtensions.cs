#if NETSTANDARD2_0

namespace System.Collections.Concurrent.Generic;

public static class ConcurrentBagExtensions
{
    /// <summary>
    /// Clear Implementation for .net Standard 2.0
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    /// <param name="bag">bag</param>
    public static void Clear<T>(ConcurrentBag<T> bag)
    {
        while (!bag.IsEmpty)
        {
            bag.TryTake(out _);
        }
    }
}
#endif
