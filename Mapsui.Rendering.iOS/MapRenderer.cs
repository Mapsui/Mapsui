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
		public bool ShowDebugInfoInMap { get; set; }

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

			Render(_target.Layer, viewport, layers, ShowDebugInfoInMap);

			CATransaction.Commit ();
		}

		public void Dispose()
		{
			if (_target.Layer.Sublayers != null) {
				foreach (var layer in _target.Layer.Sublayers) {
					layer.RemoveFromSuperLayer ();
					layer.Dispose ();
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
			return null;//!!!!
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