using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Fetcher;
//using Mapsui.Logging;
using Mapsui.Rendering;
using Mapsui.Styles;

namespace Mapsui.Layers;

public class RasterizingLayer : BaseLayer, IAsyncDataFetcher, ISourceLayer
{
    private readonly ConcurrentStack<RasterFeature> _cache;
    private readonly ILayer _layer;
    private readonly float _pixelDensity;
    private readonly object _syncLock = new();
    private bool _busy;
    private MSection? _currentSection;
    private bool _modified;
    private readonly IRenderer _rasterizer = DefaultRendererFactory.Create();
    private FetchInfo? _fetchInfo;
    public Delayer Delayer { get; } = new();
    private readonly Delayer _rasterizeDelayer = new();
    private readonly RenderFormat _renderFormat;

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
        _renderFormat = renderFormat;
        _layer = layer;
        Name = layer.Name;
        if (rasterizer != null) _rasterizer = rasterizer;
        _cache = new ConcurrentStack<RasterFeature>();
        _pixelDensity = pixelDensity;
        _layer.DataChanged += LayerOnDataChanged;
        Delayer.MillisecondsBeforeCall = 1000;
        Delayer.MillisecondsBetweenCalls = delayBeforeRasterize;
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

        _modified = true;

        // Will start immediately if it is not currently waiting. This well be in most cases.
        _rasterizeDelayer.ExecuteDelayed(Rasterize);
    }

    private void Rasterize()
    {
        if (!Enabled) return;
        if (_busy) return;
        _busy = true;
        _modified = false;

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

                if (_modified && _layer is IAsyncDataFetcher asyncDataFetcher)
                    Delayer.ExecuteDelayed(() => asyncDataFetcher.RefreshData(_fetchInfo));
            }
            finally
            {
                _busy = false;
            }
        }
    }

    public static double SymbolSize { get; set; } = 64;

    public override IEnumerable<IFeature> GetFeatures(MRect box, double resolution)
    {
        ArgumentNullException.ThrowIfNull(box);

        var features = _cache.ToArray();

        // Use a larger extent so that symbols partially outside of the extent are included
        var biggerBox = box.Grow(resolution * SymbolSize * 0.5);

        return features.Where(f => f.Raster != null && f.Raster.Intersects(biggerBox)).ToList();
    }

    public void AbortFetch()
    {
        if (_layer is IAsyncDataFetcher asyncLayer) asyncLayer.AbortFetch();
    }

    public void RefreshData(FetchInfo fetchInfo)
    {
        if (fetchInfo.Extent == null)
            return;

        if (!Enabled) return;
        if (MinVisible > fetchInfo.Resolution) return;
        if (MaxVisible < fetchInfo.Resolution) return;

        if ((_currentSection == null) ||
            (_currentSection.Resolution != fetchInfo.Section.Resolution) ||
            !_currentSection.Extent.Contains(fetchInfo.Section.Extent))
        {
            // Explicitly set the change type to discrete for rasterization
            _fetchInfo = new FetchInfo(fetchInfo.Section, fetchInfo.CRS);
            if (_layer is IAsyncDataFetcher asyncDataFetcher)
                Delayer.ExecuteDelayed(() => asyncDataFetcher.RefreshData(_fetchInfo));
            else
                Delayer.ExecuteDelayed(Rasterize);
        }
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
}
