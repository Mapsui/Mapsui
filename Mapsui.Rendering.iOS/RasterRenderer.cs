using System;
using CoreGraphics;
using Mapsui.Geometries;
using Mapsui.Styles;
using Mapsui.Providers;
using CoreAnimation;
using System.Diagnostics;
using Foundation;
using UIKit;

namespace Mapsui.Rendering.iOS
{
	public static class RasterRenderer
	{
		public static void Draw(CALayer target, IViewport viewport, IStyle style, IFeature feature)
		{
			const string styleKey = "laag";

  			if(feature[styleKey] == null) feature[styleKey] = ToiOSBitmap(feature.Geometry);

			var bitmap = (UIImage)feature [styleKey];

			var dest = WorldToScreen(viewport, feature.Geometry.GetBoundingBox());
			dest = new BoundingBox(
				dest.MinX,
				dest.MinY,
				dest.MaxX,
				dest.MaxY);

			var destination = RoundToPixel(dest);

			var tile = new CALayer
			{
				Frame = destination,
				Contents = bitmap.CGImage
			};

			target.AddSublayer(tile);
		}

		private static BoundingBox WorldToScreen(IViewport viewport, BoundingBox boundingBox)
		{
			var first = viewport.WorldToScreen(boundingBox.Min);
			var second = viewport.WorldToScreen(boundingBox.Max);

			return new BoundingBox
				(
					Math.Min(first.X, second.X),
					Math.Min(first.Y, second.Y),
					Math.Max(first.X, second.X),
					Math.Max(first.Y, second.Y)
				);
		}

		public static CGRect RoundToPixel(BoundingBox dest)
		{
			var height = (float)(Math.Round (dest.MaxY) - Math.Round (dest.MinY));

			var frame = new CGRect(
				(float)Math.Round(dest.MinX),
				(float)Math.Round(dest.MinY),
				(float)(Math.Round(dest.MaxX) - Math.Round(dest.MinX)),
				height);

			return frame;
		}

		private static void DrawRectangle(CGContext currentContext, CGRect destination, Color outlineColor)
		{
			currentContext.SetStrokeColor (outlineColor.R, outlineColor.G, outlineColor.B, outlineColor.A);
			currentContext.SetLineWidth ((nfloat)4f);
			currentContext.StrokeRect ((CGRect)destination);
		}

		private static UIImage ToiOSBitmap(IGeometry geometry)
		{
			var raster = (IRaster)geometry;
			var rasterData = NSData.FromArray(raster.Data.ToArray());
			var bitmap = UIImage.LoadFromData(rasterData);
			return bitmap;
		}
	}
}

