using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace Mapsui.Styles;

/// <summary>
/// Class for managing all bitmaps, which are registered for Mapsui drawing
/// </summary>
public sealed class ImageSourceCache : IDisposable
{
    private static ImageSourceCache? _instance;
    private readonly ConcurrentDictionary<string, Stream> _register = [];
    private bool _disposed;

    private ImageSourceCache() { }

    /// <summary>
    /// Singleton of BitmapRegistry class
    /// </summary>
    public static ImageSourceCache Instance => _instance ??= new ImageSourceCache();

    /// <inheritdoc />
    public async Task RegisterAsync(string imageSource)
    {
        var key = imageSource.ToString();
        if (_register.ContainsKey(key))
        {
            return;
        }

        var stream = await ImageFetcher.FetchStreamFromImageSourceAsync(imageSource);
        _register[imageSource.ToString()] = stream;
    }

    /// <inheritdoc />
    public Stream? Get(string key)
    {
        _register.TryGetValue(key.ToString(), out var val);
        return val;
    }

    /// <inheritdoc />
    public Stream? Unregister(string key)
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
