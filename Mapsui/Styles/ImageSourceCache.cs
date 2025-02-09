using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Mapsui.Styles;

/// <summary>
/// Class for managing all bitmaps, which are registered for Mapsui drawing
/// </summary>
public sealed class ImageSourceCache
{
    private readonly ConcurrentDictionary<string, byte[]> _register = [];

    public Task RegisterAsync(string imageSource)
    {
        return RegisterAsync(imageSource, CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task RegisterAsync(string imageSource, CancellationToken cancellationToken)
    {
        var key = imageSource;
        if (_register.ContainsKey(key))
        {
            return;
        }

        var stream = await ImageFetcher.FetchBytesFromImageSourceAsync(imageSource, cancellationToken);
        _register[imageSource] = stream;
    }

    /// <inheritdoc />
    public byte[]? Get(string key)
    {
        _register.TryGetValue(key, out var val);
        return val;
    }

    /// <inheritdoc />
    public byte[]? Unregister(string key)
    {
        _register.TryRemove(key, out var val);
        return val;
    }
}
