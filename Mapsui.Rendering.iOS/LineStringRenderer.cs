using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Rendering.iOS.ExtensionMethods;
using Mapsui.Styles;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using System.Drawing;

namespace Mapsui.Rendering.iOS
{
	public static class LineStringRenderer
	{
		public static void Draw(CALayer target, IViewport viewport, IStyle style, IFeature feature)
		{
			var lineString = ((LineString) feature.Geometry).Vertices;
			var paints = style.ToUIKit();

			foreach (var paint in paints) 
			{
				var path = ((LineString) feature.Geometry).Vertices.ToUIKit(viewport);
				var shape = new CAShapeLayer
				{
					StrokeColor = paint.CGColor,
					LineWidth = 4f,
					Path = path.CGPath
				};

				target.AddSublayer (shape);
			}
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

		private static PointF[] WorldToScreen2(IViewport viewport, PointF[] points)
		{
			var newPoints = new PointF[points.Length];
			for (var i = 0; i < points.Length; i++)
			{
				var point = viewport.WorldToScreen(points[i].X, points[i].Y);
				newPoints [i] = new PointF ((float)point.X, (float)point.Y);
			}

			return newPoints;
		}
	}
}