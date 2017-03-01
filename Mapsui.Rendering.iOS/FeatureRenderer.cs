using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui;
using Mapsui.Rendering.iOS;
using UIKit;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using CoreAnimation;

namespace XamarinRendering
{
	class FeatureRenderer //!!! : IRenderer
	{
		private UIView target;
		private static RenderQueue renderQueue;

		public FeatureRenderer(UIView target, RenderQueue queue)
		{
			this.target = target;
			renderQueue = queue;
		}

		private static Dictionary<string, List<IFeature>> _featuresForLayer = new Dictionary<string, List<IFeature>>();

		public void Render (IViewport viewport, IEnumerable<ILayer> layers)
		{
			var layerNames = new List<string> ();

			_featuresForLayer = new Dictionary<string, List<IFeature>>();

			foreach (var layer in layers)
			{
				if (layer.Enabled &&
				    layer.MinVisible <= viewport.Resolution &&
				    layer.MaxVisible >= viewport.Resolution)
				{
					var layerName = RenderFeaturesForLayer(viewport, layer);
					if(layerName != null)
						layerNames.Add (layerName);
					//renderQueue.PutLayer (layer.Name, features);
				}
			}

			foreach (var kv in _featuresForLayer) {
				renderQueue.PutLayer (kv.Key, kv.Value);
			}

			renderQueue.ResetQueue (layerNames);
		}

		private static string RenderFeaturesForLayer(IViewport viewport, ILayer layer)
		{
			if (layer.Enabled == false) return null;

			/*
			if (layer is BasicLayer) {
				var renderedFeatures = SymbolRenderer.RenderStackedLabelLayer (viewport, layer as BasicLayer);

				if (renderedFeatures != null && renderedFeatures.Count > 0){
					renderQueue.PutLayer (layer.Name, renderedFeatures);
					return layer.Name;
				}
			}
			else if (layer is LabelLayer) {
				var renderedFeatures = LabelRenderer.RenderStackedLabelLayer (viewport, layer as LabelLayer);

				if (renderedFeatures != null && renderedFeatures.Count > 0){
					renderQueue.PutLayer (layer.Name, renderedFeatures);
					return layer.Name;
				}
			} else*/ {
				var renderedFeatures = RenderVectorLayerFeatures (viewport, layer);// new List<CALayer> ();

				if (renderedFeatures != null && renderedFeatures.Count > 0){
					//renderQueue.PutLayer (layer.Name, renderedFeatures);
					_featuresForLayer.Add(layer.Name, renderedFeatures);
					return layer.Name;
				}
			}

			return null;
		}

		private static List<IFeature> RenderVectorLayerFeatures(IViewport viewport, ILayer layer)
		{
			try
			{

				var features = new List<IFeature>();
				if(layer.Name.Equals("Open Street Map"))
				{
					Console.WriteLine("");
					features = layer.GetFeaturesInView(viewport.Extent, viewport.Resolution).ToList();
				}else
				{
					features = layer.GetFeaturesInView(viewport.Extent, viewport.Resolution).ToList();
				}

				//Console.WriteLine("Layer " + layer.Name + " Features.Count = " + features.Count);

				//if(layer.Name.Equals("Open Street Map") && features.Count > 0)
					Console.WriteLine("");

				if(layer.Style != null)
				{
					var style = layer.Style; // This is the default that could be overridden by an IThemeStyle

					foreach (var feature in features)
					{
						if (layer.Style is IThemeStyle) style = (layer.Style as IThemeStyle).GetStyle(feature);
						if ((style == null) || (style.Enabled == false) || (style.MinVisible > viewport.Resolution) || (style.MaxVisible < viewport.Resolution)) continue;

						RenderFeature(viewport, style, feature, layer.Name);
					}
				}

				foreach (var feature in features)
				{
					var styles = feature.Styles ?? Enumerable.Empty<IStyle>();
					foreach (var style in styles)
					{
						if (feature.Styles != null && style.Enabled)
						{
							RenderFeature(viewport, style, feature, layer.Name);
						}
					}
				}

				return features;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return null;   
			}
		}

		private static void RenderFeature(IViewport viewport, IStyle style, IFeature feature, string layerName)
		{
			if (style is LabelStyle)
			{
				//canvas.Children.Add(LabelRenderer.RenderLabel(feature.Geometry.GetBoundingBox().GetCentroid(), new Offset(), style as LabelStyle, viewport));
			}
			else 
			{
				var styleKey = layerName;//feature.GetHashCode ().ToString ();
				var renderedGeometry = (feature[styleKey] != null) ? (CALayer)feature[styleKey] : null;

				if (renderedGeometry == null) {
					renderedGeometry = RenderGeometry (viewport, style, feature);

					renderedGeometry.ShouldRasterize = true;

					feature [styleKey] = renderedGeometry;
					feature ["first"] = true;
				} 
                else 
                {
					feature ["first"] = false;
				}
			}
		}

		private static CALayer RenderGeometry (IViewport viewport, IStyle style, IFeature feature)
		{
			if (feature.Geometry is Mapsui.Geometries.Point)
			{
			    return null;//!!!GeometryRenderer.RenderPoint (feature.Geometry as Mapsui.Geometries.Point, style, viewport);
			} else if (feature.Geometry is Polygon){
				return GeometryRenderer.RenderPolygonOnLayer (feature.Geometry as Polygon, style, viewport);
			} else if (feature.Geometry is MultiPolygon){
				return GeometryRenderer.RenderMultiPolygonOnLayer (feature.Geometry as MultiPolygon, style, viewport);
			} else if (feature.Geometry is IRaster){
				return GeometryRenderer.RenderRasterOnLayer (feature.Geometry as IRaster, style, viewport);
			}
			return null;
		}

		private static void PositionGeometry(CALayer renderedGeometry, IViewport viewport, IStyle style, IFeature feature)
		{
			if (feature.Geometry is Point){
				GeometryRenderer.PositionPoint(renderedGeometry, feature.Geometry as Point, style, viewport);
			}
			if (feature.Geometry is MultiPoint)
				return;
			if (feature.Geometry is LineString)
				return;
			if (feature.Geometry is MultiLineString)
				return;
			if (feature.Geometry is Polygon)
				return;
			if (feature.Geometry is MultiPolygon){
				GeometryRenderer.PositionMultiPolygon (renderedGeometry as CALayer, feature.Geometry as MultiPolygon, style, viewport);
			}
			if (feature.Geometry is IRaster){
				GeometryRenderer.PositionRaster(renderedGeometry, feature.Geometry.GetBoundingBox(), viewport);
			}
		}
	}
}