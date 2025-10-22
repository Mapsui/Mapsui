// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BruTile;
using BruTile.Cache;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Tiling.Extensions;
using Mapsui.Tiling.Fetcher;
using Mapsui.Tiling.Rendering;
using Mapsui.Tiling.Utilities;

namespace Mapsui.Tiling.Layers;

/// <summary>
/// Layer, which displays a map consisting of individual tiles
/// </summary>
public class TileLayer : BaseLayer, IFetchableSource, IDisposable
{
    private readonly ITileSource _tileSource;
    private readonly IRenderFetchStrategy _renderFetchStrategy;
    private readonly int _minExtraTiles;
    private readonly int _maxExtraTiles;
    private int _numberTilesNeeded;
    private readonly TileFetchPlanner _tileFetchPlanner;
    private readonly MRect? _extent;
    private readonly HttpClient? _httpClient = null;
    private readonly HttpClient? _injectedHttpClient = null;

    public event EventHandler<FetchRequestedEventArgs>? FetchRequested;

    /// <summary>
    /// Create tile layer for given tile source
    /// </summary>
    /// <param name="tileSource">Tile source to use for this layer</param>
    /// <param name="minTiles">Minimum number of tiles to cache</param>
    /// <param name="maxTiles">Maximum number of tiles to cache</param>
    /// <param name="dataFetchStrategy">Strategy to get list of tiles for given extent</param>
    /// <param name="renderFetchStrategy"></param>
    /// <param name="minExtraTiles">Number of minimum extra tiles for memory cache</param>
    /// <param name="maxExtraTiles">Number of maximum extra tiles for memory cache</param>
    /// <param name="fetchTileAsFeature">Fetch tile as feature</param>
    /// <param name="httpClient">A custom HttpClient (may include custom header parameters etc)</param>
    public TileLayer(ITileSource tileSource, int minTiles = 200, int maxTiles = 300,
        IDataFetchStrategy? dataFetchStrategy = null, IRenderFetchStrategy? renderFetchStrategy = null,
        int minExtraTiles = -1, int maxExtraTiles = -1, Func<TileInfo, Task<IFeature?>>? fetchTileAsFeature = null,
        HttpClient? httpClient = null)
    {
        _tileSource = tileSource ?? throw new ArgumentException($"{tileSource} can not null");
        MemoryCache = new MemoryCache<IFeature?>(minTiles, maxTiles);
        Style = new RasterStyle();
        Attribution.Text = _tileSource.Attribution.Text;
        Attribution.Url = _tileSource.Attribution.Url;
        _extent = _tileSource.Schema.Extent.ToMRect();
        dataFetchStrategy ??= new DataFetchStrategy(3);
        _renderFetchStrategy = renderFetchStrategy ?? new RenderFetchStrategy();
        _minExtraTiles = minExtraTiles;
        _maxExtraTiles = maxExtraTiles;
        _tileFetchPlanner = new TileFetchPlanner(MemoryCache, _tileSource.Schema,
            fetchTileAsFeature ?? ToFeatureAsync, dataFetchStrategy, this);
        _tileFetchPlanner.DataChanged += TileFetchPlanner_OnDataChanged;
        _tileFetchPlanner.PropertyChanged += TileFetchPlanner_OnPropertyChanged;
        if (httpClient != null)
        {
            // inject a custom http client
            _injectedHttpClient = httpClient;
        }
        else
        {
            _httpClient = new();
            // use default user agent
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", HttpClientTools.GetDefaultApplicationUserAgent());
        }
    }

    /// <summary>
    /// TileSource</summary>
    public ITileSource TileSource => _tileSource;

    /// <summary>
    /// Memory cache for this layer
    /// </summary>
    private MemoryCache<IFeature?> MemoryCache { get; }

    /// <inheritdoc />
    public override IReadOnlyList<double> Resolutions => _tileSource.Schema.Resolutions.Select(r => r.Value.UnitsPerPixel).ToList();

    /// <inheritdoc />
    public override MRect? Extent => _extent;

    /// <inheritdoc />
    public override IEnumerable<IFeature> GetFeatures(MRect extent, double resolution)
    {
        if (_tileSource.Schema == null) return [];
        UpdateMemoryCacheMinAndMax();
        return _renderFetchStrategy.Get(extent, resolution, _tileSource.Schema, MemoryCache);
    }

    /// <inheritdoc />
    public void ClearCache()
    {
        MemoryCache.Clear();
    }

    public FetchJob[] GetFetchJobs(int activeFetches, int availableFetchSlots)
    {
        return _tileFetchPlanner.GetFetchJobs(activeFetches, availableFetchSlots);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            MemoryCache.Dispose();
            _httpClient?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void TileFetchPlanner_OnPropertyChanged(object? sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
        if (propertyChangedEventArgs.PropertyName == nameof(Busy))
            Busy = _tileFetchPlanner.Busy;
    }

    private void UpdateMemoryCacheMinAndMax()
    {
        if (_minExtraTiles < 0 || _maxExtraTiles < 0) return;
        if (_numberTilesNeeded == _tileFetchPlanner.NumberTilesNeeded) return;

        _numberTilesNeeded = _tileFetchPlanner.NumberTilesNeeded;
        MemoryCache.MinTiles = _numberTilesNeeded + _minExtraTiles;
        MemoryCache.MaxTiles = _numberTilesNeeded + _maxExtraTiles;
    }

    private void TileFetchPlanner_OnDataChanged(object? sender, Exception? ex)
    {
        OnDataChanged(new DataChangedEventArgs(ex, Name));
    }

    private async Task<IFeature?> ToFeatureAsync(TileInfo tileInfo)
    {
        if (_tileSource is IHttpTileSource httpTileSource)
        {
            var tileData = await httpTileSource.GetTileAsync(GetHttpClient(), tileInfo).ConfigureAwait(false);
            var mRaster = ToRaster(tileInfo, tileData);
            return new RasterFeature(mRaster);
        }
        else if (_tileSource is ILocalTileSource localTileSource)
        {
            var tileData = await localTileSource.GetTileAsync(tileInfo).ConfigureAwait(false);
            var mRaster = ToRaster(tileInfo, tileData);
            return new RasterFeature(mRaster);
        }
        else
        {
            throw new NotImplementedException($"ToFeatureAsync is not implemented for this TileSource type '{_tileSource.GetType()}'. Inherit either from either '{nameof(IHttpTileSource)}' or '{nameof(ILocalTileSource)}'");
        }
    }

    private static MRaster? ToRaster(TileInfo tileInfo, byte[]? tileData)
    {
        // A TileSource may return a byte array that is null. This is currently only implemented
        // for MbTilesTileSource. It is to indicate that the tile is not present in the source,
        // although it should be given the tile schema. It does not mean the tile could not
        // be accessed because of some temporary reason. In that case it will throw an exception.
        // For Mapsui this is important because it will not try again and again to fetch it. 
        // Here we return the geometry as null so that it will be added to the tile cache. 
        // TileLayer.GetFeatureInView will have to return only the non null geometries.

        if (tileData == null) return null;
        return new MRaster(tileData, tileInfo.Extent.ToMRect());
    }

    public virtual void ViewportChanged(FetchInfo fetchInfo)
    {
        if (fetchInfo.Section.CheckIfAreaIsTooBig())
        {
            Logging.Logger.Log(Logging.LogLevel.Error, $"The area of the section is too big in the TileLayer.ViewportChanged method with parameters: Extent: {fetchInfo.Extent}, Resolution: {fetchInfo.Resolution}");
            return; // Check added for this issue: https://github.com/Mapsui/Mapsui/issues/3105
        }

        Busy = true;
        _tileFetchPlanner.ViewportChanged(fetchInfo);
    }

    protected virtual void OnFetchRequested()
    {
        FetchRequested?.Invoke(this, new FetchRequestedEventArgs(ChangeType.Discrete));
    }

    private HttpClient GetHttpClient() => (_httpClient, _injectedHttpClient) switch
    {
        (not null, _) => _httpClient!,
        (null, not null) => _injectedHttpClient!,
        (null, null) => throw new InvalidOperationException("Both member and injected HttpClient are null. This is a bug in the TileLayer code."),
    };

}
