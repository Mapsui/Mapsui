using Mapsui.Fetcher;
using Mapsui.Logging;
using System;
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
    /// <returns>If true a new image was registered and a refresh is needed. If false the image
    /// was already registered and no refresh is needed.</returns>
    public async Task<bool> TryRegisterAsync(string sourceId, string source)
    {
        if (_register.ContainsKey(sourceId))
            return false;

        var stream = await ImageFetcher.FetchBytesFromImageSourceAsync(source);
        _register[sourceId] = stream;
        return true;
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

    public void FetchAllImageData(ConcurrentDictionary<string, string> sourceToSourceId, FetchMachine fetchMachine, Action refreshGraphics)
    {
        var unregisteredImageSource = GetUnregisteredImageSources(sourceToSourceId);

        if (unregisteredImageSource.Count == 0)
        {
            return; // Don't start a thread if there are no bitmap paths to initialize.
        }

        var needsRefresh = false;
        foreach (var imageSource in unregisteredImageSource)
        {
            fetchMachine.Enqueue(async () =>
            {
                try
                {
                    if (await TryRegisterAsync(imageSource.Value, imageSource.Key))
                        needsRefresh = true;
                }
                catch (Exception ex)
                {
                    // Todo: We might need to deal with failed initializations, and possible reties, but not too many retries.
                    Logger.Log(LogLevel.Error, ex.Message, ex);
                }
            });
        }
        if (needsRefresh)
            refreshGraphics();
    }

    /// <summary>
    /// This variant is currently only used in tests. By awaiting the method the user can be sure that the images are loaded.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> FetchAllImageDataAsync(ConcurrentDictionary<string, string> sourceToSourceId)
    {
        if (sourceToSourceId.IsEmpty)
            return await Task.FromResult(false);

        foreach (var imageSource in sourceToSourceId)
        {
            try
            {
                await TryRegisterAsync(imageSource.Value, imageSource.Key).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Todo: We might need to deal with failed initializations, and possible reties, but not too many retries.
                Logger.Log(LogLevel.Error, ex.Message, ex);
            }
        }

        return await Task.FromResult(true);
    }

    private List<KeyValuePair<string, string>> GetUnregisteredImageSources(IEnumerable<KeyValuePair<string, string>> sourceIds)
    {
        return [.. sourceIds.Where(i => !_register.ContainsKey(i.Value))];
    }

}
