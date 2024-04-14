using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using Mapsui.Logging;

namespace Mapsui.Styles;

/// <summary>
/// Class for managing all bitmaps, which are registered for Mapsui drawing
/// </summary>
public sealed class BitmapRegistry : IBitmapRegistry
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

    private static int _counter = 0;

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
        if (bitmapData is Uri uri)
            return Register(uri);

        CheckBitmapData(bitmapData);

        var id = NextBitmapId();
        _register[id] = bitmapData;
        if (key != null)
        {
            _lookup[key] = id;
        }
        return id;
    }

    public int Register(Uri bitmapPath)
    {
        var key = bitmapPath.ToString();
        Stream? stream = null;
        switch (bitmapPath.Scheme)
        {
            case "embeddedresource":
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var name = assembly.GetName().Name;
                    if (name != null)
                        if (bitmapPath.LocalPath.StartsWith(name))
                        {
                            stream = assembly.GetManifestResourceStream(bitmapPath.LocalPath);
                            if (stream != null)
                                break;
                        }

                }
                break;
            case "file":
                stream = File.OpenRead(bitmapPath.LocalPath);
                break;
            default:
                try
                {
                    using HttpClientHandler handler = new HttpClientHandler { AllowAutoRedirect = true };
                    using HttpClient client = new HttpClient(handler);
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                    using HttpResponseMessage response = client.GetAsync(bitmapPath, HttpCompletionOption.ResponseHeadersRead).Result;
                    response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status is unsuccessful
                    stream = response.Content.ReadAsStreamAsync().Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, $"Could not load from uri {bitmapPath} : {ex.Message}", ex);
                }
                break;
        }

        if (stream == null)
            throw new ArgumentException("Resource not found: " + key);
        return Register(stream, key);
    }

    private static int NextBitmapId()
    {
        return Interlocked.Increment(ref _counter);
    }

    /// <summary> Unregister an existing bitmap </summary>
    /// <param name="id">Id of registered bitmap data</param>
    /// <returns>The unregistered object</returns>
    public object? Unregister(int id)
    {
        return _register.Remove(id, out var val) ?
            val :
            _parent?.Unregister(id);
    }

    /// <summary>
    /// Get bitmap data of registered bitmap
    /// </summary>
    /// <param name="id">Id of existing bitmap data</param>
    /// <returns></returns>
    public object Get(int id)
    {
        if (_register.TryGetValue(id, out var val))
        {
            return val;
        }

        return _parent?.Get(id) ?? throw new KeyNotFoundException();
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
            if (sprite.Atlas < 0 || sprite.Atlas > _counter || !_register.ContainsKey(sprite.Atlas))
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
