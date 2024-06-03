using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Mapsui.Styles;

/// <summary>
/// Class for managing all bitmaps, which are registered for Mapsui drawing
/// </summary>
public sealed class ImageSourceCache
{
    private readonly ConcurrentDictionary<string, byte[]> _register = [];

    /// <inheritdoc />
    public async Task RegisterAsync(string imageSource)
    {
        var key = imageSource.ToString();
        if (_register.ContainsKey(key))
        {
            return;
        }

        var stream = await ImageFetcher.FetchBytesFromImageSourceAsync(imageSource);
        _register[imageSource.ToString()] = stream;
    }

    /// <inheritdoc />
    public byte[]? Get(string key)
    {
        _register.TryGetValue(key.ToString(), out var val);
        return val;
    }

    /// <inheritdoc />
    public byte[]? Unregister(string key)
    {
        _register.TryRemove(key.ToString(), out var val);
        return val;
    }
}
