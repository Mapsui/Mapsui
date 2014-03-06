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
		private readonly RenderingLayer _renderingLayer;

		public MapRenderer()
		{
			_target = new UIView();
			_renderingLayer = new RenderingLayer ();

			_target.Layer.AddSublayer (_renderingLayer);
		}a

		public MapRenderer(UIView target)
		{
			_target = target;
			_renderingLayer = new RenderingLayer ();

			target.Layer.AddSublayer (_renderingLayer);
		}

		//private readonly object _syncRoot = new object();

		public void Render(IViewport viewport, IEnumerable<ILayer> layers)
		{
			if (_target.Layer.Sublayers != null) {
				foreach (var layer in _target.Layer.Sublayers) {
					layer.RemoveFromSuperLayer ();
				}
			}

			CATransaction.Begin();
			CATransaction.AnimationDuration = 0;
			//lock (_syncRoot) {
			//_renderingLayer.PrepareLayer ();
			foreach (var layer in layers) {
				if (layer.Enabled &&
				    layer.MinVisible <= viewport.Resolution &&
				    layer.MaxVisible >= viewport.Resolution) {

					RenderLayer (_target, viewport, layer);
				}
			}

			//_renderingLayer.UpdateLayer ();
			//}
			CATransaction.Commit ();
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
				/*
				var labelLayer = layer as LabelLayer;
				var canvas = LabelRenderer.RenderLabelLayer (viewport, labelLayer, features);

				if (canvas != null)
					target.Layer.AddSublayer (canvas);
					*/
				//target.Children.Add(labelLayer.UseLabelStacking
				//                    ? LabelRenderer.RenderStackedLabelLayer(viewport, labelLayer)
				//                    : LabelRenderer.RenderLabelLayer(viewport, labelLayer));
			}
			else
			{
				var canvas = RenderVectorLayer(new CALayer(), viewport, layer);

				if(canvas != null) {
					target.Layer.AddSublayer (canvas);
				}
			}
		}

		private static CALayer RenderVectorLayer (CALayer canvas, IViewport viewport, ILayer layer)
		{
			// todo:
			// find solution for try catch. Sometimes this method will throw an exception
			// when clearing and adding features to a layer while rendering

			try
			{
				//var canvas = new CALayer();
				var features = layer.GetFeaturesInView(viewport.Extent, viewport.Resolution).ToList();

				System.Diagnostics.Debug.WriteLine("Layer name: " + layer.LayerName + " feature count: " + features.Count);

				if(layer.Style != null)
				{
					var style = layer.Style; // This is the default that could be overridden by an IThemeStyle

					foreach (var feature in features)
					{
						if (layer.Style is IThemeStyle) style = (layer.Style as IThemeStyle).GetStyle(feature);
						if ((style == null) || (style.Enabled == false) || (style.MinVisible > viewport.Resolution) || (style.MaxVisible < viewport.Resolution)) continue;

						RenderFeature(canvas, viewport, style, feature, layer.LayerName);
					}
				}

				foreach (var feature in features)
				{
					var styles = feature.Styles ?? Enumerable.Empty<IStyle>();
					foreach (var style in styles)
					{
						if (feature.Styles != null && style.Enabled)
						{
							RenderFeature(canvas, viewport, style, feature, layer.LayerName);
						}
					}
				}

				return canvas;
			}
			catch (Exception e)
			{
				Console.WriteLine(layer.LayerName + " " + e.Message);
				return null;   
			}                    
		}

		private static void RenderFeature(CALayer canvas, IViewport viewport, IStyle style, IFeature feature, string layerName)
		{
			if (style is LabelStyle)
			{
				//canvas.Children.Add(LabelRenderer.RenderLabel(feature.Geometry.GetBoundingBox().GetCentroid(), new Offset(), style as LabelStyle, viewport));
			}
			else 
			{
				var styleKey = layerName;//feature.GetHashCode ().ToString ();
				var renderedGeometry = (feature[styleKey] != null) ? (CALayer)feature[styleKey] : null;

				if (layerName.Equals ("GraafMeldingLayer"))
					System.Diagnostics.Debug.WriteLine ("GraafMeldingLayer");

				if (renderedGeometry == null) {
					renderedGeometry = RenderGeometry (viewport, style, feature);
					System.Diagnostics.Debug.WriteLine ("RenderGeometry done");
					if (feature.Geometry is IRaster)
					//if (feature.Geometry is Mapsui.Geometries.Point || feature.Geometry is IRaster) // positioning only supported for point and raster
					{						//||feature.Geometry is Mapsui.Geometries.MultiPolygon)
						feature [styleKey] = renderedGeometry;
					}
				} else {
					System.Diagnostics.Debug.WriteLine ("position geom");
					PositionGeometry(renderedGeometry, viewport, style, feature);
				}
				if(renderedGeometry == null)
					System.Diagnostics.Debug.WriteLine ("renderedGeometry = null");
				canvas.AddSublayer (renderedGeometry);
			}
		}

		private static CALayer RenderGeometry (IViewport viewport, IStyle style, IFeature feature)
		{
			if (feature.Geometry is Point){
				return GeometryRenderer.RenderPoint (feature.Geometry as Point, style, viewport);
				//return null;
			}
		    if (feature.Geometry is Polygon){
		        return GeometryRenderer.RenderPolygonOnLayer (feature.Geometry as Polygon, style, viewport);
		    }
		    if (feature.Geometry is MultiPolygon){
		        //var contextRenderer = new ContextRenderer ();
		        //contextRenderer.RenderGeometry (feature.Geometry as Mapsui.Geometries.MultiPolygon, style, feature, viewport);
		        return GeometryRenderer.RenderMultiPolygonOnLayer (feature.Geometry as MultiPolygon, style, viewport);
		    }
		    if (feature.Geometry is IRaster){
				System.Diagnostics.Debug.WriteLine ("RenderGeometry as Raster");
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
				GeometryRenderer.PositionMultiPolygon (renderedGeometry, feature.Geometry as MultiPolygon, style, viewport);
			}
			if (feature.Geometry is IRaster){
				GeometryRenderer.PositionRaster(renderedGeometry, feature.Geometry.GetBoundingBox(), viewport);
			}
		}
	}
}