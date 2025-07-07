using BruTile;
using BruTile.Cache;
using Mapsui.Fetcher;
using Mapsui.Layers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Mapsui.Tiling.Fetcher;

public class TileFetchPlanner(
    ITileCache<IFeature?> tileCache,
    ITileSchema tileSchema,
    Func<TileInfo, Task<IFeature?>> fetchTileAsFeature,
    IDataFetchStrategy dataFetchStrategy,
    ILayer layer) : INotifyPropertyChanged, IFetchableSource
{
    public static int DefaultNumberOfSimultaneousFetches { get; set; } = 4;
    private bool _busy;
    private readonly IDataFetchStrategy _dataFetchStrategy = dataFetchStrategy;
    private readonly LatestMailbox<FetchInfo> _latestFetchInfo = new();
    private readonly FetchTracker _fetchTracker = new();

    public int NumberTilesNeeded { get; private set; }

    public event EventHandler<Exception?>? DataChanged;
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning disable CS0067 // Not used but required by the interface.
    public event EventHandler<FetchRequestedEventArgs>? FetchRequested;
#pragma warning restore CS0067

    public void ViewportChanged(FetchInfo fetchInfo)
    {
        // Set Busy to true immediately, so that the caller can immediately start waiting for it to go back to false.
        // Not sure if this is the best solution. It will often go to true and back to false without doing something.
        Busy = true;

        _latestFetchInfo.Overwrite(fetchInfo);
    }

    public FetchJob[] GetFetchJobs(int activeFetches, int availableFetchSlots)
    {
        if (_latestFetchInfo.TryTake(out var fetchInfo))
        {
            if (!layer.Enabled
                || layer.MaxVisible < fetchInfo.Resolution
                || layer.MinVisible > fetchInfo.Resolution)
                return [];

            NumberTilesNeeded = _fetchTracker.Update(fetchInfo, tileSchema, _dataFetchStrategy, tileCache);
        }
        Busy = !_fetchTracker.IsDone();

        var fetchCount = Math.Min(Math.Max(DefaultNumberOfSimultaneousFetches - activeFetches, 0), availableFetchSlots);

        // We want to keep a limited number of tiles in progress because the extent could change again and we do not
        // want to fetch tiles that are not needed anymore.
        var fetchJobs = new List<FetchJob>();
        while (_fetchTracker.TryTake(out var tileToFetch, fetchCount))
            fetchJobs.Add(new FetchJob(layer.Id, () => FetchOnThreadAsync(tileToFetch)));
        return fetchJobs.ToArray();
    }

    private async Task FetchOnThreadAsync(TileInfo tileInfo)
    {
        try
        {
            var feature = await fetchTileAsFeature(tileInfo).ConfigureAwait(false);
            FetchCompleted(tileInfo, feature, null);
        }
        catch (Exception ex)
        {
            // The exception is returned to the caller and should be logged there.
            FetchCompleted(tileInfo, null, ex);
        }
    }

    private void FetchCompleted(TileInfo tileInfo, IFeature? feature, Exception? exception)
    {
        if (exception != null)
        {
            _fetchTracker.FetchFailed(tileInfo.Index);
        }
        else
        {
            _fetchTracker.FetchDone(tileInfo.Index);
            tileCache.Add(tileInfo.Index, feature);
        }

        Busy = !_fetchTracker.IsDone();
        DataChanged?.Invoke(this, exception);
    }

    public bool Busy
    {
        get => _busy;
        private set
        {
            if (_busy == value)
                return;
            _busy = value;
            OnPropertyChanged(nameof(Busy));
        }
    }

    public int Id => layer.Id;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void ClearCache()
    {
        _latestFetchInfo.TryTake(out var _);
        _fetchTracker.Clear();
    }
}
