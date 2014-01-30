using System;
using System.Collections.Generic;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Rendering;

namespace Mapsui.Layers
{
    public class RasterizingLayer : BaseLayer
    {
        private readonly object _syncLock = new object();
        private readonly ILayer _layer;
        private readonly MemoryProvider _cache;
        private BoundingBox _extent;
        private double _resolution;
        private bool _invalid;

        public RasterizingLayer(ILayer layer)
        {
            _layer = layer;
            _layer.DataChanged += LayerOnDataChanged;
            _cache = new MemoryProvider();
        }

        private void LayerOnDataChanged(object sender, DataChangedEventArgs dataChangedEventArgs)
        {
            if (_invalid) return;
            _invalid = true;

            lock (_syncLock)
            {
                while (_invalid)
                {
                    _invalid = false;
                    if (double.IsNaN(_resolution)) return;
                    var viewport = CreateViewport(_extent, _resolution);
                    var renderer = RendererFactory.Get;
                    if (renderer == null) throw new Exception("No render was registered");
                    var bitmapStream = renderer().RenderToBitmapStream(viewport, new[] {_layer});

                    DisposeAllFeatures(_cache.Features);
                    _cache.Clear();
                    _cache.Features = new Features {new Feature {Geometry = new Raster(bitmapStream, viewport.Extent)}};
                }
            }
            OnDataChanged(dataChangedEventArgs);
        }

        private static void DisposeAllFeatures(IEnumerable<IFeature> features)
        {
            foreach (var feature in features)
            {
                foreach (var key in feature.RenderedGeometry.Keys)
                {
                    var geometry = feature.RenderedGeometry[key];
                    var disposable = (geometry as IDisposable);
                    if (disposable != null) disposable.Dispose();
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

        public override void ViewChanged(bool changeEnd, BoundingBox extent, double resolution)
        {
            _extent = extent;
            _resolution = resolution;
            _layer.ViewChanged(changeEnd, extent, resolution);
        }

        public override void ClearCache()
        {
            _layer.ClearCache();
        }

        private static Viewport CreateViewport(BoundingBox extent, double resolution)
        {
            return new Viewport
            {
                Resolution = resolution,
                Center = extent.GetCentroid(),
                Width = extent.Width / resolution,
                Height = extent.Height / resolution
            };
        }
    }
}
