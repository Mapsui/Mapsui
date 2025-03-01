using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
    /// <param name="image"></param>
    /// <returns>If true a new image was registered and a refresh is needed. If false the image
    /// was already registered and no refresh is needed.</returns>
    public async Task<bool> TryRegisterAsync(Image image)
    {
        var key = image.SourceId;
        if (_register.ContainsKey(key))
            return false;

        var stream = await ImageFetcher.FetchBytesFromImageSourceAsync(image.Source);
        _register[key] = stream;
        return true;
    }

    public List<Image> GetUnregisteredImageSources(List<Image> images)
    {
        var result = images
            .Where(i => !_register.ContainsKey(i.SourceId))
            .ToList();
        return result;
    }

    /// <summary>
    /// Get a image from the cache
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    public byte[]? Get(Image image)
    {
        _register.TryGetValue(image.SourceId, out var val);
        return val;
    }

    /// <summary>
    /// Unregister a image from the cache
    /// </summary>
    public byte[]? Unregister(Image image)
    {
        _register.TryRemove(image.SourceId, out var val);
        return val;
    }
}
