using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Styles;

public sealed class FontSourceCache : IFetchableSource
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, byte[]> _register = [];

    public event EventHandler<FetchRequestedEventArgs>? FetchRequested;

    public int Id => -2; // Distinct from ImageSourceCache's -1. Not a layer Id.

    /// <summary>Get cached font bytes for the given FontSource, or null if not yet loaded.</summary>
    public byte[]? Get(FontSource fontSource)
    {
        _register.TryGetValue(fontSource.SourceId, out var val);
        return val;
    }

    /// <summary>
    /// Fetch and cache the bytes for a single font source.
    /// Returns false if it was already registered.
    /// </summary>
    public async Task<bool> TryRegisterAsync(string sourceId, string source)
    {
        if (_register.ContainsKey(sourceId))
            return false;

        var bytes = await ImageFetcher.FetchBytesFromImageSourceAsync(source);
        _register[sourceId] = bytes;
        return true;
    }

    /// <summary>
    /// Fetch all font sources that have been declared but not yet loaded.
    /// This variant is currently only used in tests.
    /// </summary>
    public async Task FetchAllFontDataAsync()
    {
        foreach (var (source, sourceId) in FontSource.SourceToSourceId)
        {
            try
            {
                await TryRegisterAsync(sourceId, source).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message, ex);
            }
        }
    }

    public FetchJob[] GetFetchJobs(int activeFetchCount, int availableFetchSlots)
    {
        if (!NeedsFetching())
            return [];

        if (activeFetchCount > 0)
            return []; // Only one font fetch job at a time.

        return [new FetchJob(Id, async () => await FetchAllFontDataAsync())];
    }

    public void ViewportChanged(FetchInfo fetchInfo) { }

    public void ClearCache()
    {
        _register.Clear();
        FetchRequested?.Invoke(this, new FetchRequestedEventArgs(ChangeType.Discrete));
    }

    private bool NeedsFetching() => FontSource.SourceToSourceId.Any(kv => !_register.ContainsKey(kv.Value));
}
