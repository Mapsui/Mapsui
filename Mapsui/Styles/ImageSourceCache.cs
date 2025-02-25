using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Mapsui.Styles;

/// <summary>
/// Class for managing all bitmaps, which are registered for Mapsui drawing
/// </summary>
public sealed class ImageSourceCache
{
    private readonly ConcurrentDictionary<string, byte[]> _register = [];

    /// <summary>
    /// Register an image for drawing
    /// </summary>
    /// <param name="imageSource"></param>
    /// <returns>If true a new image was registered and a refresh is needed. If false the image
    /// was already registered and no refresh is needed.</returns>
    public async Task<bool> TryRegisterAsync(string imageSource)
    {
        var key = imageSource.ToString();
        if (_register.ContainsKey(key))
            return false;

        var stream = await ImageFetcher.FetchBytesFromImageSourceAsync(imageSource);
        _register[imageSource.ToString()] = stream;
        return true;
    }

    /// <summary>
    /// Get a image from the cache
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public byte[]? Get(string key)
    {
        _register.TryGetValue(key.ToString(), out var val);
        return val;
    }

    /// <summary>
    /// Unregister a image from the cache
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public byte[]? Unregister(string key)
    {
        _register.TryRemove(key.ToString(), out var val);
        return val;
    }
}
