using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;

namespace Mapsui.Rendering.XamlRendering
{
    /// <summary>
    /// Wrapper around a feature provider that returns a rasterized image of the features.
    /// </summary>
    public class RasterizingProvider : IProvider
    {
        private readonly object _syncLock = new object();
        private readonly ILayer _layer;


        public RasterizingProvider(ILayer layer)
        {
            _layer = layer;
        }

        public int SRID
        {
            get { return _layer.SRID; }
            set { }
        }

        public IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            lock (_syncLock)
            {
                IFeatures features = null;
                var thread = new Thread(() => RenderToRaster(CreateViewport(box, resolution), _layer, out features));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
                return features;
            }
        }

        public BoundingBox GetExtents()
        {
            return _layer.Envelope;
        }

        private static void RenderToRaster(IViewport viewport, ILayer layer, out IFeatures features)
        {
            var canvas = new Canvas();
            MapRenderer.RenderLayer(canvas, viewport, layer);
            var bitmap = MapRenderer.ToBitmapStream(canvas, viewport.Width, viewport.Height);
            features = new Features { new Feature { Geometry = new Raster(bitmap, viewport.Extent) } };
        }

        private static Viewport CreateViewport(BoundingBox box, double resolution)
        {
            return new Viewport
                {
                    Resolution = resolution,
                    Center = box.GetCentroid(),
                    Width = box.Width/resolution,
                    Height = box.Height/resolution
                };
        }
    }
}
