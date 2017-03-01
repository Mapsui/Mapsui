using Foundation;
using CoreText;
using Mapsui;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using CoreAnimation;
using System;
using System.Collections.Generic;
using CoreGraphics;
using System.Linq;

namespace XamarinRendering
{
	static class LabelRenderer
	{
        // todo: delete the code below if you don't know what it was supposed to do
		//public static List<IFeature> RenderStackedLabelLayer(IViewport viewport, LabelLayer layer)
		//{
		//	var renderedFeatures = new List<IFeature> ();
		//	var canvas = new CALayer ();
		//	canvas.Opacity = (float)layer.Opacity;

		//	//todo: take into account the priority 
		//	var features = layer.GetFeaturesInView(viewport.Extent, viewport.Resolution);
		//	var margin = viewport.Resolution * 50;

		//	if(layer.Style != null)
		//	{
		//		var clusters = new List<Cluster>();
		//		//todo: repeat until there are no more merges
		//		ClusterFeatures(clusters, features, margin, layer.Style, viewport.Resolution);

		//		foreach (var cluster in clusters)
		//		{
		//			var feature = cluster.Features.OrderBy(f => f.Geometry.GetBoundingBox().GetCentroid().Y).FirstOrDefault();
		//			//SetFeatureOutline (feature, layer.Name, cluster.Features.Count);
		//			//var bb = RenderBox(cluster.Box, viewport);

		//			//Zorg dat dit ALTIJD decimal zelfde ISet als ViewChanged is
		//			//var feature = cluster.Features.FirstOrDefault ();

		//			var styles = feature.Styles ?? Enumerable.Empty<IStyle>();
		//			foreach (var style in styles)
		//			{
		//				if (feature.Styles != null && style.Enabled)
		//				{
		//					var styleKey = layer.Name; //feature.GetHashCode ().ToString ();
		//					var renderedGeometry = (feature[styleKey] != null) ? (CALayer)feature[styleKey] : null;
		//					var labelText = layer.GetLabelText(feature);

		//					if (renderedGeometry == null) {
		//					//Mapsui.Geometries.Point point, Offset stackOffset, LabelStyle style, IFeature feature, IViewport viewport, string text)
		//						renderedGeometry = RenderLabel(feature.Geometry as Mapsui.Geometries.Point,
		//						                               style as LabelStyle, feature, viewport, labelText);

		//						feature [styleKey] = renderedGeometry;
		//						feature ["first"] = true;

		//					} else {
		//						feature ["first"] = false;
		//					}
		//				}
		//			}
		//			renderedFeatures.Add (feature);

		//			/*
		//			Offset stackOffset = null;

		//			foreach (var feature in cluster.Features.OrderBy(f => f.Geometry.GetBoundingBox().GetCentroid().Y))
		//			{
		//				if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);
		//				if ((style == null) || (style.Enabled == false) || (style.MinVisible > viewport.Resolution) || (style.MaxVisible < viewport.Resolution)) continue;

		//				if (stackOffset == null) //first time
		//				{
		//					stackOffset = new Offset();
		//					if (cluster.Features.Count > 1)
		//						canvas.AddSublayer (RenderBox(cluster.Box, viewport));
		//				}
		//				else stackOffset.Y += 18; //todo: get size from text, (or just pass stack nr)

		//				if (!(style is LabelStyle)) throw new Exception("Style of label is not a LabelStyle");
		//				var labelStyle = style as LabelStyle;
		//				string labelText = layer.GetLabel(feature);
		//				var position = new Mapsui.Geometries.Point(cluster.Box.GetCentroid().X, cluster.Box.Bottom);
		//				canvas.AddSublayer(RenderLabel(position, stackOffset, labelStyle, feature, viewport, labelText));
		//			}
		//			*/
		//		}
		//	}

		//	return renderedFeatures;
		//}

		private static CALayer RenderBox(BoundingBox box, IViewport viewport)
		{
			const int margin = 32;

			var p1 = viewport.WorldToScreen(box.Min);
			var p2 = viewport.WorldToScreen(box.Max);

			var rectangle = new CGRect {Width = (float) (p2.X - p1.X + margin), Height = (float) (p1.Y - p2.Y + margin)};

		    var canvas = new CALayer
		    {
		        Frame = rectangle,
		        BorderColor = new CoreGraphics.CGColor(0, 0, 0, 1),
		        BorderWidth = 2
		    };

		    return canvas;
		}

        // todo: delete code below if you don't know what it was supposed to do
		//public static CALayer RenderLabelLayer(IViewport viewport, LabelLayer layer, List<IFeature> features)
		//{
		//	var canvas = new CALayer();
		//	canvas.Opacity = (float)layer.Opacity;

		//	//todo: take into account the priority 
		//	var stackOffset = new Offset();

		//	if (layer.Style != null)
		//	{
		//		var style = layer.Style;

		//		foreach (var feature in features)
		//		{
		//			if (style is IThemeStyle) style = (style as IThemeStyle).GetStyle(feature);

		//			if ((style == null) || (style.Enabled == false) || (style.MinVisible > viewport.Resolution) || (style.MaxVisible < viewport.Resolution)) continue;
		//			if (!(style is LabelStyle)) throw new Exception("Style of label is not a LabelStyle");
		//			//var labelStyle = style as LabelStyle;
		//			string labelText = layer.GetLabelText(feature);

		//			var label = RenderLabel (feature.Geometry as Mapsui.Geometries.Point,
		//			                         style as LabelStyle, feature, viewport, labelText);

		//			canvas.AddSublayer(label);
		//		}
		//	}

		//	return canvas;
		//}

		private static void ClusterFeatures(
			IList<Cluster> clusters, 
			IEnumerable<IFeature> features, 
			double minDistance,
			IStyle layerStyle, 
			double resolution)
		{
			var style = layerStyle;
			//this method should repeated several times until there are no more merges
			foreach (var feature in features.OrderBy(f => f.Geometry.GetBoundingBox().GetCentroid().Y))
			{
				if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);
				if ((style == null) || (style.Enabled == false) || (style.MinVisible > resolution) || (style.MaxVisible < resolution)) 
					continue;

				var found = false;
				foreach (var cluster in clusters)
				{
					//todo: use actual overlap of labels not just proximity of geometries.
					if (cluster.Box.Grow(minDistance).Contains(feature.Geometry.GetBoundingBox().GetCentroid()))
					{
						cluster.Features.Add(feature);
						cluster.Box = cluster.Box.Join(feature.Geometry.GetBoundingBox());
						found = true;
						break;
					}
				}

				if (!found)
				{
					var cluster = new Cluster();
					cluster.Box = feature.Geometry.GetBoundingBox().Clone();
					cluster.Features = new List<IFeature>();
					cluster.Features.Add(feature);
					clusters.Add(cluster);
				}
			}
		}

		public static CALayer RenderLabel(Mapsui.Geometries.Point point, LabelStyle style, IViewport viewport)
		{
			//Offset stackOffset,
			//return RenderLabel(point, stackOffset, style, viewport, style.Text);
			return new CALayer ();
		}

		public static CATextLayer RenderLabel(Mapsui.Geometries.Point point, LabelStyle style, IFeature feature, IViewport viewport, string text)
		{
			// Offset stackOffset,
			Mapsui.Geometries.Point p = viewport.WorldToScreen(point);
			//var pointF = new xPointF((float)p.X, (float)p.Y);
			var label = new CATextLayer ();


			var aString = new Foundation.NSAttributedString (text,
			                                                           new CoreText.CTStringAttributes(){
				Font = new CoreText.CTFont("ArialMT", 10)
			});

			var frame = new CGRect(new CoreGraphics.CGPoint((int)p.X, (int)p.Y), GetSizeForText(0, aString));
			//label.Frame = frame;
			//frame.Width = (float)(p2.X - p1.X);// + margin);
			//frame.Height = (float)(p1.Y - p2.Y);

			label.FontSize = 10;
			label.ForegroundColor = new CoreGraphics.CGColor (0, 0, 255, 150);
			label.BackgroundColor = new CoreGraphics.CGColor (255, 0, 2, 150);
			label.String = text;

			label.Frame = frame;

			Console.WriteLine ("Pos " + label.Frame.X + ":" + label.Frame.Y + " w " + label.Frame.Width + " h " + label.Frame.Height);

			// = MonoTouch.UIKit.UIScreen.MainScreen.Scale;
			//	label.ContentsScale = scale;
            
			return label;
		}

		private static CGSize GetSizeForText(int width, Foundation.NSAttributedString aString)
		{
			var frameSetter = new CoreText.CTFramesetter (aString);

			Foundation.NSRange range;
			//CTFramesetterRef framesetter = CTFramesetterCreateWithAttributedString( (CFMutableAttributedStringRef) attributedString); 
			var size = (CGSize)frameSetter.SuggestFrameSize ((NSRange)new Foundation.NSRange (0, 0), (CTFrameAttributes)null,
(CGSize)			                                         new CoreGraphics.CGSize (width, Int32.MaxValue), out range);

			//CGSize suggestedSize = CTFramesetterSuggestFrameSizeWithConstraints(framesetter, CFRangeMake(0, 0), NULL, CGSizeMake(inWidth, CGFLOAT_MAX), NULL);
			//CFRelease(framesetter);
			Console.WriteLine ("Size = " + size.Width + ":" + size.Height + "Range = " + range.Length);

			return size;
		}
        
		private class Cluster
		{
			public BoundingBox Box { get; set; }
			public IList<IFeature> Features { get; set; }
		}
	}
}