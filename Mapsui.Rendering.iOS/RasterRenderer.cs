using System;
using MonoTouch.CoreGraphics;
using Mapsui.Geometries;
using System.Drawing;
using Mapsui.Styles;
using Mapsui.Providers;
using MonoTouch.CoreAnimation;
using System.Diagnostics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Mapsui.Rendering.iOS
{
	public static class RasterRenderer
	{

//		public static void Draw(CGContext currentContext, IViewport viewport, IStyle style, IFeature feature)
//		{
//			try
//			{
//				if (!feature.RenderedGeometry.ContainsKey(style)) feature.RenderedGeometry[style] = ToiOSBitmap(feature.Geometry);
//				var bitmap = (UIImage)feature.RenderedGeometry[style];
//
//				var dest = WorldToScreen(viewport, feature.Geometry.GetBoundingBox());
//				dest = new BoundingBox(
//					dest.MinX,
//					dest.MinY,
//					dest.MaxX,
//					dest.MaxY);
//
//				var destination = RoundToPixel(dest);
////				var destination = GeometryRenderer.ConvertBoundingBox(feature.Geometry.GetBoundingBox(), viewport);
////				destination = new RectangleF(0, (256 - destination.Height), destination.Width, destination.Height);
//
//				currentContext.DrawImage(destination, bitmap.CGImage);
//				//var img = UIImage.FromImage(bitmap);
//				//img.Draw(destination);
//
//				//UIGraphics.PushContext(currentContext);
//			
//				//bitmap.Draw(destination);
//				//bitmap.Dispose();
//				//DrawOutline(currentContext, style, destination);
//
//				//UIGraphics.PopContext();
//			}
//			catch (Exception ex)
//			{
//				Trace.WriteLine(ex.Message);
//			}
//		}

		public static void Draw(CALayer target, IViewport viewport, IStyle style, IFeature feature)
		{
			//if (!feature.RenderedGeometry.ContainsKey(style)) feature["laag"] = ToiOSBitmap(feature.Geometry);
			var styleKey = "laag";

			if(feature[styleKey] == null) feature[styleKey] = ToiOSBitmap(feature.Geometry);

			//if (!feature.RenderedGeometry.ContainsKey(style)) feature.RenderedGeometry[style] = ToiOSBitmap(feature.Geometry);
			var bitmap = (UIImage)feature [styleKey];//(UIImage)feature.RenderedGeometry[style];

			var dest = WorldToScreen(viewport, feature.Geometry.GetBoundingBox());
			dest = new BoundingBox(
				dest.MinX,
				dest.MinY,
				dest.MaxX,
				dest.MaxY);

			var destination = RoundToPixel(dest);

			var tile = new CALayer()
			{
				Frame = destination,
				Contents = bitmap.CGImage
			};

//			Console.WriteLine ("Destination: " + destination.X + " " + destination.Y + " " + destination.Width + " "+ destination.Height);

			target.AddSublayer(tile);
		}

		private static void DrawOutline(CGContext currentContext, IStyle style, RectangleF destination)
		{
			var vectorStyle = (style as VectorStyle);
			if (vectorStyle == null) return;
			if (vectorStyle.Outline == null) return;
			if (vectorStyle.Outline.Color == null) return;
			DrawRectangle(currentContext, destination, vectorStyle.Outline.Color);
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

		public static RectangleF RoundToPixel(BoundingBox dest)
		{
			var height = (float)(Math.Round (dest.MaxY) - Math.Round (dest.MinY));

			//height = height * -1;

			var frame = new RectangleF(
				(float)Math.Round(dest.MinX),
				(float)Math.Round(dest.MinY),
				(float)(Math.Round(dest.MaxX) - Math.Round(dest.MinX)),
				height);

			return frame;
		}

		private static void DrawRectangle(CGContext currentContext, RectangleF destination, Styles.Color outlineColor)
		{
			currentContext.SetStrokeColor (outlineColor.R, outlineColor.G, outlineColor.B, outlineColor.A);
			currentContext.SetLineWidth (4f);
			currentContext.StrokeRect (destination);
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

