using System.IO;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using MonoTouch.CoreAnimation;
using MonoTouch.UIKit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Rendering.iOS
{
	public class MapRenderer : IRenderer
	{
		private readonly UIView _target;
		//private CALayer _renderingLayer;

		public MapRenderer()
		{
			_target = new UIView();
		}

		public MapRenderer(UIView target)
		{
			_target = target;
		}

		public void Render(IViewport viewport, IEnumerable<ILayer> layers)
		{
			if (_target.Layer.Sublayers != null) {
				foreach (var layer in _target.Layer.Sublayers) {
					layer.RemoveFromSuperLayer ();
				}
			}

			CATransaction.Begin();
			CATransaction.AnimationDuration = 0;

			//Render(target, viewport, layers, ShowDebugInfoInMap);

			foreach (var layer in layers) {
				if (layer.Enabled &&
					layer.MinVisible <= viewport.Resolution &&
					layer.MaxVisible >= viewport.Resolution) {

					RenderLayer (_target, viewport, layer);
				}
			}

			CATransaction.Commit ();
		}

		private static void Render(CALayer target, IViewport viewport, IEnumerable<ILayer> layers, bool showDebugInfoInMap)
		{
//			layers = layers.ToList();
//			VisibleFeatureIterator.IterateLayers(viewport, layers, (v, s, f) => RenderGeometry(target, v, s, f));
		}

		public MemoryStream RenderToBitmapStream(IViewport viewport, IEnumerable<ILayer> layers)
		{
			return null;//!!!!
		}

		private void RenderLayer(UIView target, IViewport viewport, ILayer layer)
		{
			if (layer.Enabled == false) return;

			if (layer is LabelLayer)
			{
				var labelLayer = layer as LabelLayer;
			}
			else
			{
				RenderVectorLayer(target.Layer, viewport, layer);
			}
		}

		private static void RenderVectorLayer (CALayer canvas, IViewport viewport, ILayer layer)
		{
			// todo:
			// find solution for try catch. Sometimes this method will throw an exception
			// when clearing and adding features to a layer while rendering

			try
			{
				//var canvas = new CALayer();
				var features = layer.GetFeaturesInView(viewport.Extent, viewport.Resolution).ToList();
				//System.Diagnostics.Debug.WriteLine("Layer name: " + layer.LayerName + " feature count: " + features.Count);

				if(layer.Style != null)
				{
					var style = layer.Style; // This is the default that could be overridden by an IThemeStyle

					foreach (var feature in features)
					{
						if (layer.Style is IThemeStyle) style = (layer.Style as IThemeStyle).GetStyle(feature);
						if ((style == null) || (style.Enabled == false) || (style.MinVisible > viewport.Resolution) || (style.MaxVisible < viewport.Resolution)) continue;

						RenderGeometry(canvas, viewport, style, feature);
					}
				}

				foreach (var feature in features)
				{
					var styles = feature.Styles ?? Enumerable.Empty<IStyle>();
					foreach (var style in styles)
					{
						if (feature.Styles != null && style.Enabled)
						{
							RenderGeometry(canvas, viewport, style, feature);
						}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("RenderVectorLayer Exception: " + layer.LayerName + " " + e.Message);
				Console.WriteLine("RenderVectorLayer Stacktrace: " + layer.LayerName + " " + e.StackTrace);
			}                    
		}

		private static void RenderGeometry (CALayer target, IViewport viewport, IStyle style, IFeature feature)
		{
			if (feature.Geometry is Mapsui.Geometries.Point) {
				PointRenderer.Draw (target, viewport, style, feature);
			} else if (feature.Geometry is LineString) {
				LineStringRenderer.Draw (target, viewport, style, feature);
			} else if (feature.Geometry is Polygon){
				GeometryRenderer.RenderPolygonOnLayer (feature.Geometry as Polygon, style, viewport);
			} else if (feature.Geometry is MultiPolygon){
				GeometryRenderer.RenderMultiPolygonOnLayer (feature.Geometry as MultiPolygon, style, viewport);
			} else if (feature.Geometry is IRaster) {
				RasterRenderer.Draw (target, viewport, style, feature);
			}
		}
	}
}