using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Mapsui.Logging;

namespace Mapsui.Styles;

/// <summary>
/// Class for managing all bitmaps, which are registered for Mapsui drawing
/// </summary>
public sealed class ImagePathCache : IDisposable
{
    private static ImagePathCache? _instance;
    private readonly ConcurrentDictionary<string, Stream> _register = [];
    private bool _disposed;

    private ImagePathCache() { }

    /// <summary>
    /// Singleton of BitmapRegistry class
    /// </summary>
    public static ImagePathCache Instance => _instance ??= new ImagePathCache();

    /// <inheritdoc />
    public async Task RegisterAsync(Uri bitmapPath)
    {
        var key = bitmapPath.ToString();
        if (_register.ContainsKey(key))
        {
            Logger.Log(LogLevel.Warning, $"Bitmap already registered: '{key}'");
        }

        var stream = await ImageFetcher.FetchStreamFromImagePathAsync(bitmapPath);
        _register[bitmapPath.ToString()] = stream;
    }

    /// <inheritdoc />
    public Stream? Get(Uri key)
    {
        _register.TryGetValue(key.ToString(), out var val);
        return val;
    }

    /// <inheritdoc />
    public Stream? Unregister(Uri key)
    {
        _register.TryRemove(key.ToString(), out var val);
        return val;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var it in _register)
        {
            if (it.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        _register.Clear();

        _disposed = true;
    }
}
