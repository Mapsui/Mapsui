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
		static MapRenderer()
        {
            DefaultRendererFactory.Create = () => new MapRenderer();
        }
        
        

        public void Render(object target, IViewport viewport, IEnumerable<ILayer> layers, Color background = null)
		{
            var view = (UIView)target;

            CATransaction.Begin();
			CATransaction.AnimationDuration = 0;
			RemoveSublayers(view.Layer);
			Render(view.Layer, viewport, layers);

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

        // do I need something like this? perhaps after each render iteration?
        //      public void Dispose()
        //      {
        //          if (target.Layer.Sublayers != null)
        //          {
        //              foreach (var layer in target.Layer.Sublayers)
        //              {
        //                  layer.RemoveFromSuperLayer();
        //                  layer.Dispose();
        //              }
        //          }
        //      }

 		static void Render(CALayer target, IViewport viewport, IEnumerable<ILayer> layers)
		{
			layers = layers.ToList();
			VisibleFeatureIterator.IterateLayers(viewport, layers, (v, s, f) => RenderGeometry(target, v, s, f));
		}

        MemoryStream IRenderer.RenderToBitmapStream(IViewport viewport, IEnumerable<ILayer> layers, Color background)
        { 
            UIImage image = null;
            var handle = new ManualResetEvent(false);

            var view = new UIView();
            view.InvokeOnMainThread(() =>
            {
				try
				{
					view.Opaque = false;
					view.BackgroundColor = new UIColor(background.R, background.G, background.B, background.A);
					Render(view, viewport, layers, background);
					image = ToImage(view, new CGRect(0, 0, (float)viewport.Width, (float)viewport.Height));
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Exception in {nameof(IRenderer.RenderToBitmapStream)}: {ex}");
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