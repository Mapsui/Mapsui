using System;
using MonoTouch.CoreAnimation;
using Mapsui.Styles;
using Mapsui.Providers;
using Mapsui.Geometries;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace Mapsui.Rendering.iOS
{
	static class LineStringRenderer
	{
		public static CALayer Draw(IViewport viewport, IStyle style, IFeature feature)
		{
			var shapeLayer = new CAShapeLayer ();

			var vectorStyle = style as VectorStyle;
			if (vectorStyle == null)
				return null;

			SetStyle (shapeLayer, vectorStyle);

			var path = ((LineString) feature.Geometry).Vertices.ToUIKit(viewport);
			shapeLayer.Path = path.CGPath;
			//
			//			var paints = style.ToiOS();
			//			//using (var paint = new Paint {Color = Color.Black, StrokeWidth = 8, AntiAlias = true})
			//			foreach (var paint in paints)
			//			{
			//				var vertices = lineString;
			//				var points = vertices.ToiOS();
			//				WorldToScreen(viewport, points);
			//
			//				var line = new UIBezierPath ();
			//				foreach(var point in points){
			//					line.AddLineTo (point);
			//				}
			//
			//				paint.Dispose();
			//				shapeLayer.Path = line.CGPath;
			//			}

			return shapeLayer;
		}

		private static void SetStyle(CAShapeLayer symbol, VectorStyle style)
		{
			if (style.Fill != null && style.Fill.Color != null)
			{
				float fillAlpha = (float)style.Fill.Color.A / 255;
				var fillColor = new CGColor(new CGColor(style.Fill.Color.R, style.Fill.Color.G,
					style.Fill.Color.B), fillAlpha);
				symbol.FillColor = fillColor;
			}
			else
			{
				symbol.BackgroundColor = new CGColor(0, 0, 0, 0);
			}

			if (style.Outline != null)
			{
				float strokeAlpha = (float)style.Outline.Color.A / 255;

				var strokeColor = new CGColor(style.Outline.Color.R, style.Outline.Color.G,
					style.Outline.Color.B, strokeAlpha);
				//symbol.BorderColor = strokeColor;
				//symbol.BorderWidth = (float)style.Outline.Width;
				symbol.LineWidth = (float)style.Outline.Width;
				symbol.StrokeColor = strokeColor;
			}
			else
			{
				float strokeAlpha = 1;
				var strokeColor = new CGColor(0, 0, 0);
				//symbol.BorderColor = strokeColor;
				//symbol.BorderWidth = (float)style.Outline.Width;
				symbol.LineWidth = 2f;
				symbol.StrokeColor = strokeColor;
			}
		}

		private static void WorldToScreen(IViewport viewport, PointF[] points)
		{
			for (var i = 0; i < points.Length; i++)
			{
				var point = viewport.WorldToScreen(points[i].X, points[i].Y);
				points [i] = new PointF ((float)point.X, (float)point.Y);
			}
		}
	}
}