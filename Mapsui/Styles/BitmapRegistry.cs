using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Extensions;
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
    private int _counter;

    private BitmapRegistry() { }

    /// <summary>
    /// Singleton of BitmapRegistry class
    /// </summary>
    public static BitmapRegistry Instance => _instance ??= new BitmapRegistry();

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
    public async Task<int> RegisterAsync(Uri bitmapPath)
    {
        var key = bitmapPath.ToString();
        if (TryGetBitmapId(key, out var id))
        {
            Logger.Log(LogLevel.Warning, $"Bitmap already registered: '{key}'");
            return id;
        }
        var stream = bitmapPath.Scheme switch
        {
            "embeddedresource" => LoadEmbeddedResourceFromPath(bitmapPath),
            "file" => LoadFromFileSystem(bitmapPath),
            "http" or "https" => await LoadFromUrlAsync(bitmapPath),
            _ => throw new ArgumentException($"Scheme is not supported '{bitmapPath.Scheme}' of '{bitmapPath}'"),
        };
        return Register(stream, key);
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

    private static Stream LoadEmbeddedResourceFromPath(Uri bitmapPath)
    {
        try
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var name = assembly.GetName().Name;
                if (name is null)
                    throw new Exception($"Assembly name is null: '{assembly}'");
                if (bitmapPath.Host.StartsWith(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    string[] resourceNames = assembly.GetManifestResourceNames();
                    var realResourceName = resourceNames.FirstOrDefault(r => r.Equals(bitmapPath.Host, StringComparison.InvariantCultureIgnoreCase));
                    if (realResourceName != null)
                    {
                        using var stream = assembly.GetManifestResourceStream(realResourceName);
                        if (stream is null)
                            throw new Exception($"The resource name was found but GetManifestResourceStream returned null: '{bitmapPath}'");
                        return stream.CopyToMemoryStream(); // Copy stream to memory stream to avoid issues with disposing the stream
                    }
                }
            }
            throw new Exception($"Could not find the embedded resource in the CurrentDomain.GetAssemblies(): '{bitmapPath}'");
        }
        catch (Exception ex)
        {
            var message = $"Could not load embedded resource '{bitmapPath}' : '{ex.Message}'";
            Logger.Log(LogLevel.Error, message, ex);
            throw new Exception(message, ex);
        }
    }

    private async static Task<Stream> LoadFromUrlAsync(Uri bitmapPath)
    {
        try
        {
            using HttpClientHandler handler = new HttpClientHandler { AllowAutoRedirect = true };
            using var httpClient = new HttpClient(handler);
            using HttpResponseMessage response = await httpClient.GetAsync(bitmapPath, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status is unsuccessful
            await using var tempStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return tempStream.CopyToMemoryStream(); // Copy stream to memory stream to avoid issues with disposing the stream
        }
        catch (Exception ex)
        {
            var message = $"Could not load resource from url '{bitmapPath}' : '{ex.Message}'";
            Logger.Log(LogLevel.Error, message, ex);
            throw new Exception(message, ex);
        }
    }

    private static Stream LoadFromFileSystem(Uri bitmapPath)
    {
        try
        {
            return File.OpenRead(bitmapPath.LocalPath);
        }
        catch (Exception ex)
        {
            var message = $"Could not load resource from file '{bitmapPath}' : '{ex.Message}'";
            Logger.Log(LogLevel.Error, message, ex);
            throw new Exception(message, ex);
        }
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

        if (bitmapData is Sprite sprite)
        {
            if (sprite.Atlas < 0 || !_register.ContainsKey(sprite.Atlas))
            {
                throw new ArgumentException("Sprite has no corresponding atlas bitmap.");
            }
        }
    }
}
