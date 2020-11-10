using System.Linq;
using System;
using System.Collections.Generic;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Logging;
using Mapsui.Providers;
using Mapsui.Rendering;

namespace Mapsui.Layers
{
    public class RasterizingLayer : BaseLayer, IAsyncDataFetcher
    {
        private readonly MemoryProvider _cache;
        private readonly ILayer _layer;
        private readonly bool _onlyRerasterizeIfOutsideOverscan;
        private readonly double _overscan;
        private readonly double _renderResolutionMultiplier;
        private readonly float _pixelDensity;
        private readonly object _syncLock = new object();
        private bool _busy;
        private Viewport _currentViewport;
        private BoundingBox _extent;
        private bool _modified;
        private IEnumerable<IFeature> _previousFeatures;
        private IRenderer _rasterizer;
        private double _resolution;
        public Delayer Delayer { get; } = new Delayer();
        private Delayer _rasterizeDelayer = new Delayer();

        /// <summary>
        ///     Creates a RasterizingLayer which rasterizes a layer for performance
        /// </summary>
        /// <param name="layer">The Layer to be rasterized</param>
        /// <param name="delayBeforeRasterize">Delay after viewport change to start rerasterising</param>
        /// <param name="renderResolutionMultiplier"></param>
        /// <param name="rasterizer">Rasterizer to use. null will use the default</param>
        /// <param name="overscanRatio">The ratio of the size of the rasterized output to the current viewport</param>
        /// <param name="onlyRerasterizeIfOutsideOverscan">
        ///     Set the rasterization policy. false will trigger a Rasterization on
        ///     every viewport change. true will trigger a Rerasterization only if the viewport moves outside the existing
        ///     rasterization.
        /// </param>
        public RasterizingLayer(
            ILayer layer,
            int delayBeforeRasterize = 1000,
            double renderResolutionMultiplier = 1,
            IRenderer rasterizer = null,
            double overscanRatio = 1,
            bool onlyRerasterizeIfOutsideOverscan = false,
            float pixelDensity = 1)
        {
            if (overscanRatio < 1)
                throw new ArgumentException($"{nameof(overscanRatio)} must be >= 1", nameof(overscanRatio));

            _layer = layer;
            Name = layer.Name;
            _renderResolutionMultiplier = renderResolutionMultiplier;
            _rasterizer = rasterizer;
            _cache = new MemoryProvider();
            _overscan = overscanRatio;
            _onlyRerasterizeIfOutsideOverscan = onlyRerasterizeIfOutsideOverscan;
            _pixelDensity = pixelDensity;
            _layer.DataChanged += LayerOnDataChanged;
            Delayer.StartWithDelay = true;
            Delayer.MillisecondsToWait = delayBeforeRasterize;
        }

        public override BoundingBox Envelope => _layer.Envelope;

        public ILayer ChildLayer => _layer;

        private void LayerOnDataChanged(object sender, DataChangedEventArgs dataChangedEventArgs)
        {
            if (!Enabled) return;
            if (MinVisible > _resolution) return;
            if (MaxVisible < _resolution) return;
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
                    if (double.IsNaN(_resolution) || _resolution <= 0) return;
                    if (_extent.Width <= 0 || _extent.Height <= 0) return;
                    var viewport = CreateViewport(_extent, _resolution, _renderResolutionMultiplier, _overscan);

                    _currentViewport = viewport;

                    _rasterizer = _rasterizer ?? DefaultRendererFactory.Create();

                    var bitmapStream = _rasterizer.RenderToBitmapStream(viewport, new[] { _layer }, pixelDensity: _pixelDensity);
                    RemoveExistingFeatures();

                    if (bitmapStream != null)
                    {
                        _cache.ReplaceFeatures(new Features
                        {
                            new Feature {Geometry = new Raster(bitmapStream, viewport.Extent)}
                        });

#if DEBUG
                        Logger.Log(LogLevel.Debug, $"Memory after rasterizing layer {GC.GetTotalMemory(true):N0}");
#endif

                        OnDataChanged(new DataChangedEventArgs());
                    }

                    if (_modified) Delayer.ExecuteDelayed(() => _layer.RefreshData(_extent.Copy(), _resolution, ChangeType.Discrete));
                }
                finally
                {
                    _busy = false;
                }
            }
        }

        private void RemoveExistingFeatures()
        {
            var features = _cache.Features.ToList();
            _cache.Clear(); // clear before dispose to prevent possible null disposed exception on render

            // Disposing previous and storing current in the previous field to prevent dispose during rendering.
            if (_previousFeatures != null) DisposeRenderedGeometries(_previousFeatures);
            _previousFeatures = features;
        }

        private static void DisposeRenderedGeometries(IEnumerable<IFeature> features)
        {
            foreach (var feature in features)
            {
                var raster = feature.Geometry as Raster;
                raster?.Data?.Dispose();

                foreach (var key in feature.RenderedGeometry.Keys)
                {
                    var disposable = feature.RenderedGeometry[key] as IDisposable;
                    disposable?.Dispose();
                }
            }
        }

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox extent, double resolution)
        {
            return _cache.GetFeaturesInView(extent, resolution);
        }

        public void AbortFetch()
        {
            if (_layer is IAsyncDataFetcher asyncLayer) asyncLayer.AbortFetch();
        }

        public override void RefreshData(BoundingBox extent, double resolution, ChangeType changeType)
        {
            var newViewport = CreateViewport(extent, resolution, _renderResolutionMultiplier, 1);

            if (!Enabled) return;
            if (MinVisible > resolution) return;
            if (MaxVisible < resolution) return;

            if (!_onlyRerasterizeIfOutsideOverscan ||
                (_currentViewport == null) ||
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                (_currentViewport.Resolution != newViewport.Resolution) ||
                !_currentViewport.Extent.Contains(newViewport.Extent))
            {
                _extent = extent;
                _resolution = resolution;
                if (_layer is IAsyncDataFetcher)
                    Delayer.ExecuteDelayed(() => _layer.RefreshData(extent.Copy(), resolution, changeType));
                else
                    Delayer.ExecuteDelayed(Rasterize);
            }
        }

        public void ClearCache()
        {
            if (_layer is IAsyncDataFetcher asyncLayer) asyncLayer.ClearCache();
        }

        private static Viewport CreateViewport(BoundingBox extent, double resolution, double renderResolutionMultiplier,
            double overscan)
        {
            var renderResolution = resolution / renderResolutionMultiplier;
            return new Viewport
            {
                Resolution = renderResolution,
                Center = extent.Centroid,
                Width = extent.Width * overscan / renderResolution,
                Height = extent.Height * overscan / renderResolution
            };
        }
    }
}