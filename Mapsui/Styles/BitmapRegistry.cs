using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Mapsui.Styles;

/// <summary>
/// Class for managing all bitmaps, which are registered for Mapsui drawing
/// </summary>
public sealed class BitmapRegistry : IDisposable
{
    private static BitmapRegistry? _instance;
    private readonly ConcurrentDictionary<int, object> _register = [];
    private readonly ConcurrentDictionary<string, int> _lookup = [];
    private int _counter;

    private BitmapRegistry() { }

    /// <summary>
    /// Singleton of BitmapRegistry class
    /// </summary>
    public static BitmapRegistry Instance => _instance ??= new BitmapRegistry();

    /// <inheritdoc />
    public int Register(string bitmapData, string? key = null) => Register((object)bitmapData, key);

    public int Register(Stream bitmapData, string? key = null) => Register((object)bitmapData, key);

    public int Register(Sprite bitmapData, string? key = null) => Register((object)bitmapData, key);

    /// <inheritdoc />
    public int Register(object bitmapData, string? key = null)
    {
        CheckBitmapData(bitmapData);

        var id = NextBitmapId();
        _register[id] = bitmapData;
        if (key != null)
        {
            _lookup[key] = id;
        }
        return id;
    }


    /// <inheritdoc />
    public object Get(int id)
    {
        return _register[id];
    }

    /// <inheritdoc />
    public bool TryGetBitmapId(string key, out int bitmapId)
    {
        if (_lookup.TryGetValue(key, out bitmapId))
        {
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public bool Update(int id, object bitmapData)
    {
        CheckBitmapData(bitmapData);

        if (id < 0 || id > _counter && !_register.ContainsKey(id))
            return false;

        _register.TryGetValue(id, out var oldBitmap);
        _register[id] = bitmapData;
        if (oldBitmap is IDisposable disposable)
        {
            disposable.Dispose();
        }

        return true;
    }

    /// <inheritdoc />
    public object? Unregister(int id)
    {
        _register.TryRemove(id, out var val);
        TryRemoveLookup(_lookup, id);
        return val;
    }

    private static void TryRemoveLookup(ConcurrentDictionary<string, int> lookup, int id)
    {
        var kpv = lookup.FirstOrDefault(kpv => kpv.Value == id);
        if (!kpv.Equals(default(KeyValuePair<string, int>)))
            lookup.TryRemove(kpv.Key, out _);
    }

    public void Dispose()
    {
        _lookup.Clear();
        foreach (var it in _register)
        {
            if (it.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        _register.Clear();
    }

    private int NextBitmapId()
    {
        return Interlocked.Increment(ref _counter);
    }

    /// <summary>
    /// Check bitmap data for correctness
    /// </summary>
    /// <param name="bitmapData">Bitmap data to check</param>
    private void CheckBitmapData(object bitmapData)
    {
        if (bitmapData == null)
            throw new ArgumentException("The bitmap data that is registered is null. Was the image loaded correctly?");

        if (bitmapData is Sprite)
        {
            throw new Exception("A bitmap stream should never be a Sprite. The Sprite class has a different purpose after Mapsui 5.0.0-beta.1.");
        }
    }
}
