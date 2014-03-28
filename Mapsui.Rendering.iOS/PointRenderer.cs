using System;
using Mapsui.Styles;
using Mapsui.Providers;
using Mapsui.Geometries;
using MonoTouch.CoreGraphics;
using System.Drawing;
using MonoTouch.CoreAnimation;
using MonoTouch.UIKit;

namespace Mapsui.Rendering.iOS
{
	public static class PointRenderer
	{
		public static void Draw(CALayer target, IViewport viewport, IStyle style, IFeature feature)
		{
			var point = feature.Geometry as Mapsui.Geometries.Point;
			var dest = viewport.WorldToScreen(point);

			var paints = style.ToiOS();

			foreach (var paint in paints)
			{
				var path = UIBezierPath.FromRoundedRect(new RectangleF((float)dest.X, (float)dest.Y, 20, 20), 10);
				var shape = new CAShapeLayer()
				{
					FillColor = paint.CGColor,
					Path = path.CGPath
				};

				shape.BorderColor = UIColor.Purple.CGColor;
				shape.BorderWidth = 20;

				target.AddSublayer(shape);
			}
		}

		private static void DrawOutline(CGContext currentContext, IStyle style, RectangleF destination)
		{
			var vectorStyle = (style as VectorStyle);
			if (vectorStyle == null) return;
			if (vectorStyle.Outline == null) return;
			if (vectorStyle.Outline.Color == null) return;
			DrawRectangle(currentContext, destination, vectorStyle.Outline.Color);
		}

		private static void DrawRectangle(CGContext currentContext, RectangleF destination, Styles.Color outlineColor)
		{
			currentContext.SetStrokeColor (outlineColor.R, outlineColor.G, outlineColor.B, outlineColor.A);
			currentContext.SetLineWidth (4f);
			currentContext.StrokeRect (destination);
		}
	}
}

