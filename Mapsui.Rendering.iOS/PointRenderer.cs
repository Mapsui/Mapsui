using System;
using Mapsui.Styles;
using Mapsui.Providers;
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
			var point = feature.Geometry as Geometries.Point;
			var dest = viewport.WorldToScreen(point);
            //var path = UIBezierPath.FromRect(new Rectangle((int)dest.X, (int)dest.Y, 20, 20));
            var path = CGPath.FromRect(new RectangleF((float)dest.X, (float)dest.Y, 20, 20), CGAffineTransform.MakeIdentity());
            

            style = new VectorStyle();
            if (!feature.RenderedGeometry.Keys.Contains(style))
		    {
		        feature.RenderedGeometry[style] = CreateRenderedPoint();
		    }
            else
            {
                Console.WriteLine("test");
            }
		    var renderedGeometry = (CAShapeLayer) feature.RenderedGeometry[style];
		    renderedGeometry.Path = path;
            target.AddSublayer(renderedGeometry);
		}

	    private static CAShapeLayer CreateRenderedPoint()
	    {
	        return new CAShapeLayer
	        {
	            FillColor = UIColor.Red.CGColor,
	            BorderColor = UIColor.Purple.CGColor,
                BorderWidth = 20,
	        };
	    }

	    private static void DrawOutline(CGContext currentContext, IStyle style, RectangleF destination)
		{
			var vectorStyle = (style as VectorStyle);
			if (vectorStyle == null) return;
			if (vectorStyle.Outline == null) return;
			if (vectorStyle.Outline.Color == null) return;
			DrawRectangle(currentContext, destination, vectorStyle.Outline.Color);
		}

		private static void DrawRectangle(CGContext currentContext, RectangleF destination, Color outlineColor)
		{
			currentContext.SetStrokeColor (outlineColor.R, outlineColor.G, outlineColor.B, outlineColor.A);
			currentContext.SetLineWidth (4f);
			currentContext.StrokeRect (destination);
		}
	}
}

