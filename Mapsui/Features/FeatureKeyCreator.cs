using System.Threading;
using Mapsui.Cache;

namespace Mapsui.Features;
public class FeatureKeyCreator<T> where T : notnull
{
    private readonly LruCache<T, uint> _keys;
    private int _nextKey;

    /// <summary> Number of Keys to be created </summary>
    /// <param name="numberOfKey">number of keys</param>
    public FeatureKeyCreator(int numberOfKey = 100000)    
    {
        _keys = new LruCache<T, uint>(numberOfKey);
    }

    public uint GetKey(T key)
    {
        if (_keys.TryGetValue(key, out var value))
            return value;

        Interlocked.Increment(ref _nextKey);
        var nextKey = (uint)_nextKey;
        _keys.Put(key, nextKey);
        return nextKey;
    }
}
