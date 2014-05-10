using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using MonoTouch.CoreAnimation;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using Point = Mapsui.Geometries.Point;

namespace Mapsui.Rendering.iOS
{
    public class MapRenderer : IRenderer
    {
        private readonly UIView _target;
        public bool ShowDebugInfoInMap { get; set; }

        static MapRenderer()
        {
            DefaultRendererFactory.Create = () => new MapRenderer();
        }

        public MapRenderer() : this(new UIView()) { }

        public MapRenderer(UIView target)
        {
            _target = target;
        }

        public void Render(IViewport viewport, IEnumerable<ILayer> layers)
        {
            Render(_target, viewport, layers, ShowDebugInfoInMap);
        }

        public static void Render(UIView target, IViewport viewport, IEnumerable<ILayer> layers, bool showDebugInfoInMap)
        {
            CATransaction.Begin();
            CATransaction.AnimationDuration = 0;
            
            if (target.Layer.Sublayers != null)
            {
                foreach (var layer in target.Layer.Sublayers)
                {
                    layer.RemoveFromSuperLayer();
                }
            }
            
            Render(target.Layer, viewport, layers);

            CATransaction.Commit();
        }

        public void Dispose()
        {
            if (_target.Layer.Sublayers != null)
            {
                foreach (var layer in _target.Layer.Sublayers)
                {
                    layer.RemoveFromSuperLayer();
                    layer.Dispose();
                }
            }
        }

        private static void Render(CALayer target, IViewport viewport, IEnumerable<ILayer> layers)
        {
            layers = layers.ToList();
            VisibleFeatureIterator.IterateLayers(viewport, layers, (v, s, f) => RenderGeometry(target, v, s, f));
        }

        public MemoryStream RenderToBitmapStream(IViewport viewport, IEnumerable<ILayer> layers)
        {
            return RenderToBitmapStreamStatic(viewport, layers);
        }

        private static MemoryStream RenderToBitmapStreamStatic(IViewport viewport, IEnumerable<ILayer> layers)
        {
            UIImage image = null;
            var handle = new ManualResetEvent(false);

            var view = new UIView();
            view.InvokeOnMainThread(() =>
            {
                view.Opaque = false;
                view.BackgroundColor = UIColor.Clear;
                Render(view, viewport, layers, false);
                image = ToImage(view, new RectangleF(0, 0, (float)viewport.Width, (float)viewport.Height));
                handle.Set();
            });

            handle.WaitOne();
            using (var nsdata = image.AsPNG())
            {
                return new MemoryStream(nsdata.ToArray());
            }
        }

        private static UIImage ToImage(UIView view, RectangleF frame)
        {
            UIGraphics.BeginImageContext(frame.Size);
            UIColor.Clear.SetColor();
            UIGraphics.RectFill(view.Frame);
            view.Layer.RenderInContext(UIGraphics.GetCurrentContext());
            var image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return image;
        }

        private static void RenderGeometry(CALayer target, IViewport viewport, IStyle style, IFeature feature)
        {
            if (feature.Geometry is Point)
            {
                PointRenderer2.RenderPoint(target, (Point)feature.Geometry, style, viewport,feature);
            }
            else if (feature.Geometry is LineString)
            {
                LineStringRenderer.Draw(target, viewport, style, feature);
            }
            else if (feature.Geometry is Polygon)
            {
                GeometryRenderer.RenderPolygonOnLayer(feature.Geometry as Polygon, style, viewport);
            }
            else if (feature.Geometry is MultiPolygon)
            {
                GeometryRenderer.RenderMultiPolygonOnLayer(feature.Geometry as MultiPolygon, style, viewport);
            }
            else if (feature.Geometry is IRaster)
            {
                RasterRenderer.Draw(target, viewport, style, feature);
            }
        }
    }
}