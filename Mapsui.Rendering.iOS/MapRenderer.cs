using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using CoreAnimation;
using UIKit;
using System.Collections.Generic;
using CoreGraphics;
using System.IO;
using System.Linq;
using System.Threading;
using CGPoint = Mapsui.Geometries.Point;
using System;
using System.Diagnostics;

namespace Mapsui.Rendering.iOS
{
    public class MapRenderer : IRenderer
    {
        public bool ShowDebugInfoInMap { get; set; }

		readonly UIView _target;

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
			RemoveSublayers(target.Layer);
			Render(target.Layer, viewport, layers);

			CATransaction.Commit();
		}

		static void RemoveSublayers(CALayer parent)
		{
			if (parent.Sublayers != null)
			{
				foreach (var layer in parent.Sublayers)
				{
					RemoveSublayers(layer);
					layer.RemoveFromSuperLayer();
					layer.Dispose();
				}
			}
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

        public MemoryStream RenderToBitmapStream(IViewport viewport, IEnumerable<ILayer> layers)
        {
            return RenderToBitmapStreamStatic(viewport, layers);
        }

		static void Render(CALayer target, IViewport viewport, IEnumerable<ILayer> layers)
		{
			layers = layers.ToList();
			VisibleFeatureIterator.IterateLayers(viewport, layers, (v, s, f) => RenderGeometry(target, v, s, f));
		}

        static MemoryStream RenderToBitmapStreamStatic(IViewport viewport, IEnumerable<ILayer> layers)
        {
            UIImage image = null;
            var handle = new ManualResetEvent(false);

            var view = new UIView();
            view.InvokeOnMainThread(() =>
            {
				try
				{
					view.Opaque = false;
					view.BackgroundColor = UIColor.Clear;
					Render(view, viewport, layers, false);
					image = ToImage(view, new CGRect(0, 0, (float)viewport.Width, (float)viewport.Height));
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Exception in {nameof(RenderToBitmapStreamStatic)}: {ex}");
				}
				finally
				{
					handle.Set();
				}
            });

            handle.WaitOne();
            using (var nsdata = image.AsPNG())
            {
                return new MemoryStream(nsdata.ToArray());
            }
        }

        static UIImage ToImage(UIView view, CGRect frame)
        {
            UIGraphics.BeginImageContext(frame.Size);
            UIColor.Clear.SetColor();
            UIGraphics.RectFill(view.Frame);
            view.Layer.RenderInContext(UIGraphics.GetCurrentContext());
            var image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return image;
        }

        static void RenderGeometry(CALayer target, IViewport viewport, IStyle style, IFeature feature)
        {
            if (feature.Geometry is CGPoint)
            {
                PointRenderer2.RenderPoint(target, (CGPoint)feature.Geometry, style, viewport,feature);
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