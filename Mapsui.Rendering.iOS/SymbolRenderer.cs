using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using System.Globalization;
using Mapsui;
using MonoTouch.CoreAnimation;
using System.Drawing;
using xPointF = System.Drawing.PointF;

namespace XamarinRendering
{
	public static class SymbolRenderer
	{
		private static readonly object _syncRoot = new object();

		public static List<IFeature> RenderStackedLabelLayer(IViewport viewport, BasicLayer layer)
		{
			lock(_syncRoot)
			{
				var renderedFeatures = new List<IFeature> ();
				var canvas = new CALayer ();
				canvas.Opacity = (float)layer.Opacity;

				//todo: take into account the priority 
				var features = layer.GetFeaturesInView (viewport.Extent, viewport.Resolution);
				var margin = viewport.Resolution * 15;

				var clusters = new List<Cluster> ();
				//todo: repeat until there are no more merges
				ClusterFeatures (clusters, features, margin, null, viewport, viewport.Resolution);
				//CenterClusters (clusters);

				//CATransaction.Begin();
				//CATransaction.SetValueForKey (MonoTouch.Foundation.NSNumber.FromBoolean(true), CATransaction.DisableActionsKey);

				foreach(var cluster in clusters){

					var feature = cluster.Features.OrderBy(f => f.Geometry.GetBoundingBox().GetCentroid().Y).FirstOrDefault();
					//SetFeatureOutline (feature, layer.LayerName, cluster.Features.Count);
					//var bb = RenderBox(cluster.Box, viewport);

					//Zorg dat dit ALTIJD decimal zelfde ISet als ViewChanged is
					//var feature = cluster.Features.FirstOrDefault ();

					var styles = feature.Styles ?? Enumerable.Empty<IStyle>();
					foreach (var style in styles)
					{
						if (feature.Styles != null && style.Enabled)
						{
							var styleKey = layer.LayerName; //feature.GetHashCode ().ToString ();
							var renderedGeometry = (feature[styleKey] != null) ? (CALayer)feature[styleKey] : null;

							if (renderedGeometry == null) {
								renderedGeometry = GeometryRenderer.RenderPoint (feature.Geometry as Mapsui.Geometries.Point, style, viewport);

								feature [styleKey] = renderedGeometry;
								feature ["first"] = true;

							} else {
								feature ["first"] = false;
							}
						}
					}
					renderedFeatures.Add (feature);

					//renderedFeatures.Add (bb);
				}

				return renderedFeatures;
			}
		}

		private static CALayer RenderBox(BoundingBox box, IViewport viewport)
		{
			//const int margin = 32;
			//const int halfMargin = margin / 2;

			var p1 = viewport.WorldToScreen(box.Min);
			var p2 = viewport.WorldToScreen(box.Max);

			var rectangle = new RectangleF();
			rectangle.Width = (float)(p2.X - p1.X);// + margin);
			rectangle.Height = (float)(p1.Y - p2.Y);// + margin);

			var v = GeometryRenderer.ConvertBoundingBox (box, viewport);

			var canvas = new CALayer ();

			canvas.Frame = v;
			canvas.BorderColor = new MonoTouch.CoreGraphics.CGColor (255, 255, 255, 1);
			canvas.BorderWidth = 2;

			return canvas;
		}

		private static void SetFeatureOutline(IFeature feature,string layerName, int featureCount)
		{
			var renderedGeometry = feature[layerName];

			if(renderedGeometry != null){
				var caLayer = renderedGeometry as MonoTouch.CoreAnimation.CALayer;

				if (featureCount == 1) {
					caLayer.BorderColor = new MonoTouch.CoreGraphics.CGColor (0, 0, 0, 0);
				} else if(featureCount > 1 && featureCount < 4) {
					caLayer.BorderColor = new MonoTouch.CoreGraphics.CGColor (0, 100, 100, 255);
				} else if(featureCount > 4 && featureCount <= 8) {
					caLayer.BorderColor = new MonoTouch.CoreGraphics.CGColor (0, 0, 255, 255);
				} else if(featureCount > 8 && featureCount <= 16) {
					caLayer.BorderColor = new MonoTouch.CoreGraphics.CGColor (0, 255, 0, 255);
				} else if(featureCount > 16) {
					caLayer.BorderColor = new MonoTouch.CoreGraphics.CGColor (255, 0, 0, 255);
				}
			} else {
				var styles = feature.Styles ?? Enumerable.Empty<IStyle>();
				var style = styles.FirstOrDefault () as SymbolStyle;

				if (style == null)
					style = new SymbolStyle ();

				if(featureCount > 1 && featureCount < 4) {
					style.Outline = new Pen { Width = 2, Color = new Color { A = 255, R = 100, G = 100, B = 0 } };
				} else if(featureCount > 4 && featureCount <= 8) {
					style.Outline = new Pen { Width = 2, Color = new Color { A = 255, R = 0, G = 0, B = 255 } };
				} else if(featureCount > 8 && featureCount <= 16) {
					style.Outline = new Pen { Width = 2, Color = new Color { A = 255, R = 0, G = 255, B = 0 } };
				} else if(featureCount > 16) {
					style.Outline = new Pen { Width = 2, Color = new Color { A = 255, R = 255, G = 0, B = 0 } };
				}
			}
		}

		public static void ClusterFeatures(
			IList<Cluster> clusters, 
			IEnumerable<IFeature> features, 
			double minDistance,
			IStyle layerStyle,
			IViewport viewport,
			double resolution)
		{
			//var style = layerStyle;
			//this method should repeated several times until there are no more merges

			//GetFeatures (features);

			foreach (var feature in features)//.OrderBy(f => f.Geometry.GetBoundingBox().GetCentroid().Y))
			{
				var style = feature.Styles.FirstOrDefault ();
				if (style != null) {
					var symbolStyle = style as SymbolStyle;
					symbolStyle.Outline = null;
				}
				//if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);
				//if ((style == null) || (style.Enabled == false) || (style.MinVisible > resolution) || (style.MaxVisible < resolution)) continue;

				var bb = GetFeatureBoundingBox (feature, viewport);
				var found = false;
				foreach (var cluster in clusters) {
					//todo: use actual overlap of labels not just proximity of geometries.
					//if (cluster.Box.Grow(minDistance).Contains(feature.Geometry.GetBoundingBox().GetCentroid()))
					if (cluster.Box.Intersects(bb))
					{
						cluster.Features.Add (feature);
						cluster.Box = cluster.Box.Join (feature.Geometry.GetBoundingBox());
						found = true;
						break;
					}
				}

				if (!found)
				{
					var cluster = new Cluster();
					cluster.Box = bb;//feature.Geometry.GetBoundingBox().Clone();
					cluster.Features = new List<IFeature>();
					cluster.Features.Add(feature);
					clusters.Add(cluster);
				}
			}

			//CenterClusters (clusters, viewport);
		}

		/*
		public static void CenterClusters(IEnumerable<Cluster> clusters, IViewport viewport)
		{
			foreach (var cluster in clusters)
			{
				var feature = cluster.Features.FirstOrDefault ();
				var center = cluster.Box.GetCentroid ();

				var styles = feature.Styles ?? Enumerable.Empty<IStyle>();
				var style = styles.FirstOrDefault () as SymbolStyle;

				var min = viewport.WorldToScreen (cluster.Box.Left, cluster.Box.Bottom);
				var max = viewport.WorldToScreen (cluster.Box.Right, cluster.Box.Top);
				//style.Width = .Width;
				//style.Height = cluster.Box.Height;

				var size = (int)Math.Min ((max.X - min.X), (min.Y - max.Y));

				//Console.WriteLine ("Size = " + size);
				//style.Width = size;
				//style.Height = size;

				feature.Geometry = center;

				//var fCenter = firstFeature.Geometry.GetBoundingBox ().GetCentroid ();
				//if(fCenter.X == cluster.Box.GetCentroid().X)
			}
		}
		*/

		private static BoundingBox GetFeatureBoundingBox(IFeature feature, 
		                                   IViewport viewport)
		{
			var styles = feature.Styles ?? Enumerable.Empty<IStyle>();
			var symbolStyle = styles.FirstOrDefault () as SymbolStyle;
			var boundingBox = feature.Geometry.GetBoundingBox ();
			//var width = style.Width;

			//var frame = GeometryRenderer.ConvertPointBoundingBox (style, feature.Geometry.GetBoundingBox (), viewport); 
			var screenMin = viewport.WorldToScreen(boundingBox.Min);
			var screenMax = viewport.WorldToScreen(boundingBox.Max);

			var min = new Mapsui.Geometries.Point(screenMin.X - (symbolStyle.Width / 2), screenMax.Y - (symbolStyle.Height / 2));
			var max = new Mapsui.Geometries.Point((min.X + symbolStyle.Width), (min.Y + symbolStyle.Height));
	
			var x = min.X;
			var y = min.Y;
			var width = max.X - min.X;
			var height = max.Y - min.Y;

			var frame = new RectangleF((float)x, (float)y, (float)width, (float)height);

			var nmin = viewport.ScreenToWorld (frame.Left, frame.Bottom);
			var nmax = viewport.ScreenToWorld (frame.Right, frame.Top);


			var bb = new BoundingBox (nmin, nmax);


			return bb;
		}

		public class Cluster
		{
			public BoundingBox Box { get; set; }
			public IList<IFeature> Features { get; set; }
		}
	}
}

