using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using Mapsui.Styles;

namespace Mapsui.Rendering.XamlRendering
{
    /// <summary>
    /// Wrapper around a feature provider that returns a rasterized image of the features.
    /// </summary>
    public class RasterizingProvider : IProvider
    {
        private readonly object _syncLock = new object();
        private readonly InMemoryLayer _layer = new InMemoryLayer();

        public RasterizingProvider(IProvider provider, IStyle style = null)
        {
            _layer.DataSource = provider;
            _layer.Style = style;
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
                var viewport = CreateViewport(box, resolution);
                foreach (var feature in _layer.GetFeaturesInView(box, resolution)) 
                {
                    // hack: clear cach to prevent cross thread exception. 
                    // todo: remove this caching mechanis.
                    feature.RenderedGeometry.Clear();   
                }
                IFeatures features = null;
                var thread = new Thread(() => RenderToRaster(viewport, _layer, out features));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Priority = ThreadPriority.Lowest;
                thread.Start();
                thread.Join();
                return features;
            }
        }

        public BoundingBox GetExtents()
        {
            return _layer.Envelope;
        }

        private void RenderToRaster(IViewport viewport, ILayer layer, out IFeatures features)
        {
            var canvas = new Canvas();
            MapRenderer.RenderLayer(canvas, viewport, layer);
            var bitmap = Utilities.ToBitmapStream(canvas, viewport.Width, viewport.Height);
            features = new Features { new Feature { Geometry = new Raster(bitmap, viewport.Extent) } };
            canvas.UpdateLayout();
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
