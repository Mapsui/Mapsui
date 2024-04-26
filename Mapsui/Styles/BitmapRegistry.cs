using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Logging;

namespace Mapsui.Styles;

/// <summary>
/// Class for managing all bitmaps, which are registered for Mapsui drawing
/// </summary>
public class BitmapRegistry
{
    private static BitmapRegistry? _instance;
    private readonly ConcurrentDictionary<int, object> _register = [];
    private readonly ConcurrentDictionary<string, int> _lookup = [];
    private readonly IBitmapRegistry? _parent;
    private BitmapRegistry() { }

    public BitmapRegistry(IBitmapRegistry parent)
    {
        _parent = parent;
    }

    private int _counter;
    private readonly ConcurrentDictionary<string, (Assembly assembly, string realResourceName)> resourceCache = new();

    /// <summary>
    /// Singleton of BitmapRegistry class
    /// </summary>
    public static BitmapRegistry Instance => _instance ??= new BitmapRegistry();

    /// <summary>
    /// Register a new bitmap
    /// </summary>
    /// <param name="bitmapData">Bitmap data to register</param>
    /// <param name="key">key for accessing bitmap</param>
    /// <returns>Id of registered bitmap data</returns>
    public int Register(object bitmapData, string? key = null)
    {
        CheckBitmapData(bitmapData);

        var id = _counter;
        _counter++;
        _register[id] = bitmapData;
        if (key != null)
        {
            _lookup[key] = id;
        }
        return id;
    }

    /// <summary> Unregister an existing bitmap </summary>
    /// <param name="id">Id of registered bitmap data</param>
    /// <returns>The unregistered object</returns>
    public object? Unregister(int id)
    {
        _register.TryGetValue(id, out var val);
        _register.Remove(id);
        return val;
    }

    /// <summary>
    /// Get bitmap data of registered bitmap
    /// </summary>
    /// <param name="id">Id of existing bitmap data</param>
    /// <returns></returns>
    public object Get(int id)
    {
        return _register[id];
    }

    /// <summary>
    /// Set new bitmap data for a already registered bitmap
    /// </summary>
    /// <param name="id">Id of existing bitmap data</param>
    /// <param name="bitmapData">New bitmap data to replace</param>
    /// <returns>True, if replacing worked correct</returns>
    public bool Set(int id, object bitmapData)
    {
        CheckBitmapData(bitmapData);

        if (id < 0 || id > _counter && !_register.ContainsKey(id))
            return _parent?.Set(id, bitmapData) ?? false;

        _register.TryGetValue(id, out var oldBitmap);
        _register[id] = bitmapData;
        if (oldBitmap is IDisposable disposable)
        {
            disposable.Dispose();
        }

        return true;
    }

    /// <summary>
    /// Check bitmap data for correctness
    /// </summary>
    /// <param name="bitmapData">Bitmap data to check</param>
    public void CheckBitmapData(object bitmapData)
    {
        if (bitmapData == null)
            throw new ArgumentException("The bitmap data that is registered is null. Was the image loaded correctly?");

        if (bitmapData is Sprite sprite)
        {
            if (sprite.Atlas < 0 || !_register.ContainsKey(sprite.Atlas))
            {
                if (_parent != null)
                    _parent.CheckBitmapData(bitmapData);
                else
                    throw new ArgumentException("Sprite has no corresponding atlas bitmap.");
            }
        }
    }

    /// <summary> Try Get Bitmap Id </summary>
    /// <param name="key">key</param>
    /// <param name="bitmapId">bitmap id</param>
    /// <returns>true if found</returns>
    public bool TryGetBitmapId(string key, out int bitmapId)
    {
        if (_lookup.TryGetValue(key, out bitmapId))
        {
            return true;
        }

        return _parent?.TryGetBitmapId(key, out bitmapId) ?? false;
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
}
