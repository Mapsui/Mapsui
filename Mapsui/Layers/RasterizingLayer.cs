using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Fetcher;
using Mapsui.Rendering;
using Mapsui.Styles;

namespace Mapsui.Layers;

public class RasterizingLayer : BaseLayer, IFetchJobSource, ISourceLayer
{
    private readonly ConcurrentStack<RasterFeature> _cache;
    private readonly ILayer _layer;
    private readonly float _pixelDensity;
    private readonly object _syncLock = new();
    private bool _busy;
    private MSection? _currentSection;
    private readonly IRenderer _rasterizer = DefaultRendererFactory.Create();
    private readonly RenderFormat _renderFormat;
    private FetchInfo? _fetchInfo;
    private readonly LatestMailbox<FetchInfo> _latestFetchInfo = new();

    public event EventHandler<Navigator.FetchRequestedEventArgs>? FetchRequested;

    /// <summary>
    ///     Creates a RasterizingLayer which rasterizes a layer for performance
    /// </summary>
    /// <param name="layer">The Layer to be rasterized</param>
    /// <param name="delayBeforeRasterize">Delay after viewport change to start re-rasterizing</param>
    /// <param name="rasterizer">Rasterizer to use. null will use the default</param>
    /// <param name="pixelDensity"></param>
    /// <param name="renderFormat">render Format png is default and skp is skia picture</param>
    public RasterizingLayer(
        ILayer layer,
        int delayBeforeRasterize = 1000,
        IRenderer? rasterizer = null,
        float pixelDensity = 1,
        RenderFormat renderFormat = RenderFormat.Png)
    {
        _renderFormat = renderFormat;
        _layer = layer;
        Name = layer.Name;
        if (rasterizer != null)
            _rasterizer = rasterizer;
        _cache = new ConcurrentStack<RasterFeature>();
        _pixelDensity = pixelDensity;
        _layer.DataChanged += LayerOnDataChanged;
        Style = new RasterStyle(); // default raster style
    }

    public override MRect? Extent => _layer.Extent;

    public ILayer SourceLayer => _layer;

    private void LayerOnDataChanged(object sender, DataChangedEventArgs dataChangedEventArgs)
    {
        if (!Enabled) return;
        if (_fetchInfo == null) return;
        if (MinVisible > _fetchInfo.Resolution) return;
        if (MaxVisible < _fetchInfo.Resolution) return;
        if (_busy) return;

        OnFetchRequested();
    }

    private async Task RasterizeAsync()
    {
        if (!Enabled) return;
        if (_busy) return;
        _busy = true;

        lock (_syncLock)
        {
            try
            {
                if (_fetchInfo == null) return;
                if (double.IsNaN(_fetchInfo.Resolution) || _fetchInfo.Resolution <= 0) return;
                if (_fetchInfo.Extent == null || _fetchInfo.Extent?.Width <= 0 || _fetchInfo.Extent?.Height <= 0) return;

                _currentSection = _fetchInfo.Section;

                using var bitmapStream = _rasterizer.RenderToBitmapStream(ToViewport(_currentSection),
                    [_layer], pixelDensity: _pixelDensity, renderFormat: _renderFormat);

                _cache.Clear();
                var features = new RasterFeature[1];
                features[0] = new RasterFeature(new MRaster(bitmapStream.ToArray(), _currentSection.Extent));
                _cache.PushRange(features);
                OnDataChanged(new DataChangedEventArgs(Name));
            }
            finally
            {
                _busy = false;
            }
        }
        await Task.CompletedTask;
    }

    public static double SymbolSize { get; set; } = 64;

    public override IEnumerable<IFeature> GetFeatures(MRect box, double resolution)
    {
        ArgumentNullException.ThrowIfNull(box);

        var features = _cache.ToArray();

        // Use a larger extent so that symbols partially outside of the extent are included
        var biggerBox = box.Grow(resolution * SymbolSize * 0.5);

        return features.Where(f => f.Raster != null && f.Raster.Extent.Intersects(biggerBox)).ToList();
    }


    public void ClearCache()
    {
        if (_layer is IAsyncDataFetcher asyncLayer) asyncLayer.ClearCache();
    }

    public static Viewport ToViewport(MSection section)
    {
        return new Viewport(
            section.Extent.Centroid.X,
            section.Extent.Centroid.Y,
            section.Resolution,
            0,
            section.ScreenWidth,
            section.ScreenHeight);
    }

    public FetchJob[] GetFetchJobs(int activeFetchCount, int availableFetchSlots)
    {
        if (_latestFetchInfo.TryTake(out var fetchInfo))
        {
            _fetchInfo = fetchInfo;

            if (_layer is IFetchJobSource fetchJobSource)
                return [new FetchJob(_layer.Id, async () =>
                    {
                        var fetchJobs = fetchJobSource.GetFetchJobs(activeFetchCount, availableFetchSlots);
                        foreach (var fetchJob in fetchJobs)
                        {
                            await fetchJob.FetchFunc();
                        }
                        await RasterizeAsync();
                    })];
            else
                return [new FetchJob(_layer.Id, RasterizeAsync)];

        }
        return [];
    }

    public void ViewportChanged(FetchInfo fetchInfo)
    {
        _latestFetchInfo.Overwrite(fetchInfo);
        if (_layer is IFetchJobSource fetchJobSource)
            fetchJobSource.ViewportChanged(fetchInfo);
    }

    protected virtual void OnFetchRequested()
    {
        FetchRequested?.Invoke(this, new Navigator.FetchRequestedEventArgs(ChangeType.Discrete));
    }
}
