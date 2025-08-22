using Mapsui.Fetcher;
using Mapsui.Layers;
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
public sealed class ImageSourceCache : IFetchableSource
{
    private readonly ConcurrentDictionary<string, byte[]> _register = [];

    public event EventHandler<FetchRequestedEventArgs>? FetchRequested;

    public int Id => -1; // This hacky! All layers have a unique Id provided by the BaseLayer. The -1 of the ImageSourceCache will not collide with the layer Ids, but if we add more IFetchableSources we might need to change this.

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

    public FetchJob[] GetFetchJobs(int activeFetchCount, int availableFetchSlots)
    {
        if (!NeedsFetching(Image.SourceToSourceId)) // This is inefficient. Perhaps we should work with queue of un-fetched images that would be empty in most cases.
            return [];

        if (activeFetchCount > 0)
            return []; // We currently do only one fetch.

        return new[] {
            new FetchJob(Id, async () =>
                {
                    _ = await FetchAllImageDataAsync(Image.SourceToSourceId);
                })
        };
    }

    public void ViewportChanged(FetchInfo fetchInfo) { } // Currently not used in ImageSourceCache, but required by the interface.

    public void ClearCache()
    {
        _register.Clear();
        OnFetchRequested();
    }

    private void OnFetchRequested()
    {
        FetchRequested?.Invoke(this, new FetchRequestedEventArgs(ChangeType.Discrete));
    }

    private bool NeedsFetching(IEnumerable<KeyValuePair<string, string>> sourceIds)
    {
        return sourceIds.Any(i => !_register.ContainsKey(i.Value));
    }
}
