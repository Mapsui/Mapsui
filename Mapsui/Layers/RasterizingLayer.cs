using System.Linq;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Rendering;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Mapsui.Layers
{
    public class RasterizingLayer : BaseLayer
    {
        private readonly object _syncLock = new object();
        private readonly ILayer _layer;
        private readonly MemoryProvider _cache;
        private BoundingBox _extent;
        private double _resolution;
        protected Timer TimerToStartRasterizing;
        private readonly int _delayBeforeRaterize;
        private IEnumerable<IFeature> _previousFeatures;
        private readonly double _renderResolutionMultiplier;
        private readonly IRenderer _rasterizer;

        public RasterizingLayer(ILayer layer, int delayBeforeRasterize = 500, double renderResolutionMultiplier = 1, IRenderer rasterizer = null)
        {
            _layer = layer;
            _delayBeforeRaterize = delayBeforeRasterize;
            _renderResolutionMultiplier = renderResolutionMultiplier;
            _rasterizer = rasterizer;
            TimerToStartRasterizing = new Timer(TimerToStartRasterizingElapsed, null, _delayBeforeRaterize, int.MaxValue);
            _layer.DataChanged += LayerOnDataChanged;
            _cache = new MemoryProvider();
        }

        void TimerToStartRasterizingElapsed(object state)
        {
            TimerToStartRasterizing.Dispose();
            Rasterize();
        }
        
        private void LayerOnDataChanged(object sender, DataChangedEventArgs dataChangedEventArgs)
        {
            StartTimerToTriggerRasterize();
        }

        private void StartTimerToTriggerRasterize()
        {
            // Postpone the request by disposing the old and creating a new Timer.
            TimerToStartRasterizing.Dispose();
            TimerToStartRasterizing = new Timer(TimerToStartRasterizingElapsed, null, _delayBeforeRaterize, int.MaxValue);
        }

        private void Rasterize()
        {
            if (!Enabled) return;

            lock (_syncLock)
            {
                if (double.IsNaN(_resolution) || _resolution <= 0) return;
                var viewport = CreateViewport(_extent, _resolution, _renderResolutionMultiplier);

                var rasterizer = _rasterizer ?? DefaultRendererFactory.Create();

                var bitmapStream = rasterizer.RenderToBitmapStream(viewport, new[] { _layer });
                RemoveExistingFeatures();
                _cache.Features = new Features {new Feature {Geometry = new Raster(bitmapStream, viewport.Extent)}};

                OnDataChanged(new DataChangedEventArgs());
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
                foreach (var key in feature.RenderedGeometry.Keys)
                {
                    var disposable = (feature.RenderedGeometry[key] as IDisposable);
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }

        public override BoundingBox Envelope
        {
            get { return _layer.Envelope; }
        }

        public override IEnumerable<IFeature> GetFeaturesInView(BoundingBox extent, double resolution)
        {
            return _cache.GetFeaturesInView(extent, resolution);
        }

        public override void AbortFetch()
        {
            _layer.AbortFetch();
        }

        public override void ViewChanged(bool majorChange, BoundingBox extent, double resolution)
        {
            _extent = extent;
            _resolution = resolution;
            _layer.ViewChanged(majorChange, extent, resolution);
            StartTimerToTriggerRasterize();
        }

        public override void ClearCache()
        {
            _layer.ClearCache();
        }

        private static Viewport CreateViewport(BoundingBox extent, double resolution, double renderResolutionMultiplier)
        {
            var renderResolution = resolution / renderResolutionMultiplier;
            return new Viewport
            {
                Resolution = renderResolution,
                Center = extent.GetCentroid(),
                Width = extent.Width / renderResolution,
                Height = extent.Height / renderResolution
            };
        }
    }
}
