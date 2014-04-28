using System;
using System.Drawing;
using System.Net.Mime;
using System.Threading;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Point = Mapsui.Geometries.Point;

namespace Mapsui.Rendering.iOS
{
    public class MapRenderer : IRenderer
    {
        private readonly UIView _target;
        public bool ShowDebugInfoInMap { get; set; }

        public MapRenderer() : this(new UIView()) { }

        public MapRenderer(UIView target)
        {
            _target = target;
            RendererFactory.Get = () => this;
        }

        public void Render(IViewport viewport, IEnumerable<ILayer> layers)
        {
            CATransaction.Begin();
            CATransaction.AnimationDuration = 0;
            
            if (_target.Layer.Sublayers != null)
            {
                foreach (var layer in _target.Layer.Sublayers)
                {
                    layer.RemoveFromSuperLayer();
                }
            }
            
            Render(_target.Layer, viewport, layers, ShowDebugInfoInMap);

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

        private static void Render(CALayer target, IViewport viewport, IEnumerable<ILayer> layers, bool showDebugInfoInMap)
        {
            layers = layers.ToList();
            VisibleFeatureIterator.IterateLayers(viewport, layers, (v, s, f) => RenderGeometry(target, v, s, f));
        }

        public MemoryStream RenderToBitmapStream(IViewport viewport, IEnumerable<ILayer> layers)
        {
            UIImage image = null;
            var handle = new ManualResetEvent(false);

            _target.InvokeOnMainThread(() =>
            {
                _target.Opaque = false;
                _target.BackgroundColor = UIColor.Clear;

                Render(viewport, layers);
                image = ToImage(_target);
                handle.Set();
            });
            handle.WaitOne();
            return new MemoryStream(image.AsPNG().ToArray());
        }
        
        private UIImage ToImage(UIView view)
        {
            UIGraphics.BeginImageContext(view.Frame.Size);
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
                PointRenderer.Draw(target, viewport, style, feature);
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