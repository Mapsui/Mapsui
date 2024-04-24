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
public sealed class BitmapRegistry : IBitmapRegistry
{
    private static BitmapRegistry? _instance;
    private readonly ConcurrentDictionary<int, object> _register = [];
    private readonly ConcurrentDictionary<string, int> _lookup = [];
    private BitmapRegistry() { }

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

        var id = NextBitmapId();
        _register[id] = bitmapData;
        if (key != null)
        {
            _lookup[key] = id;
        }
        return id;
    }

    public async Task<int> RegisterAsync(Uri bitmapPath)
    {
        var key = bitmapPath.ToString();
        Stream? stream = null;
        switch (bitmapPath.Scheme)
        {
            case "embeddedresource":
                if (resourceCache.TryGetValue(bitmapPath.Host, out var found))
                {
                    stream = found.assembly.GetManifestResourceStream(found.realResourceName);
                }
                else
                {
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        var name = assembly.GetName().Name;
                        if (name != null)
                            if (bitmapPath.Host.StartsWith(name, StringComparison.InvariantCultureIgnoreCase))
                            {
                                string[] resourceNames = assembly.GetManifestResourceNames();
                                var realResourceName = resourceNames.FirstOrDefault(r => r.Equals(bitmapPath.Host, StringComparison.InvariantCultureIgnoreCase));
                                if (realResourceName != null)
                                {
                                    stream = assembly.GetManifestResourceStream(realResourceName);
                                    if (stream != null)
                                    {
                                        resourceCache[bitmapPath.Host] = (assembly, realResourceName);
                                        break;
                                    }
                                }
                            }
                    }
                }

                break;
            case "file":
                stream = File.OpenRead(bitmapPath.LocalPath);
                break;
            case "http":
            case "https":
                try
                {
                    using HttpClientHandler handler = new HttpClientHandler { AllowAutoRedirect = true };
                    using HttpClient client = new HttpClient(handler);
                    using HttpResponseMessage response = await client.GetAsync(bitmapPath, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status is unsuccessful
                    await using var tempStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    // copy stream to memory stream to avoid issues with disposing the stream
                    stream = new MemoryStream();
                    await tempStream.CopyToAsync(stream).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, $"Could not load from uri {bitmapPath} : {ex.Message}", ex);
                }
                break;
            default:
                throw new ArgumentException($"Unsupported scheme {bitmapPath.Scheme} on {nameof(bitmapPath)}");
        }

        if (stream == null)
            throw new ArgumentException("Resource not found: " + key);
        return Register(stream, key);
    }

    public int NextBitmapId()
    {
        return Interlocked.Increment(ref _counter);
    }

    /// <summary> Unregister an existing bitmap </summary>
    /// <param name="id">Id of registered bitmap data</param>
    /// <returns>The unregistered object</returns>
    public object? Unregister(int id)
    {
        _register.Remove(id, out var val);
        return val;
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

        throw new ArgumentException("Bitmap not found: " + id);
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
            return false;

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
    private void CheckBitmapData(object bitmapData)
    {
        if (bitmapData == null)
            throw new ArgumentException("The bitmap data that is registered is null. Was the image loaded correctly?");

        if (bitmapData is Sprite sprite)
        {
            if (sprite.Atlas < 0 || !_register.ContainsKey(sprite.Atlas))
            {
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

        return false;
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
