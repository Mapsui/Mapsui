using Mapsui.Geometries;
using Mapsui.Styles;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;
using System;
using System.Collections.Generic;
using CoreGraphics;
using System.IO;
using CGPoint = Mapsui.Geometries.Point;

namespace Mapsui.Rendering.iOS
{
    static class GeometryRenderer
    {
        private static readonly IDictionary<IStyle, UIImage> BitmapCache = new Dictionary<IStyle, UIImage>();

        public static CALayer RenderPolygonOnLayer(Polygon polygon, IStyle style, IViewport viewport)
        {
            var tile = new CAShapeLayer();

            if (!(style is VectorStyle)) throw new ArgumentException("Style is not of type VectorStyle");
            var vectorStyle = style as VectorStyle;

            var strokeAlpha = (float)vectorStyle.Outline.Color.A / 255;
            var fillAlpha = (float)vectorStyle.Fill.Color.A / 255;

            var strokeColor = new CGColor(new CGColor(vectorStyle.Outline.Color.R, vectorStyle.Outline.Color.G,
                vectorStyle.Outline.Color.B), strokeAlpha);
            var fillColor = new CGColor(new CGColor(vectorStyle.Fill.Color.R, vectorStyle.Fill.Color.G,
                vectorStyle.Fill.Color.B), fillAlpha);

            tile.StrokeColor = strokeColor;
            tile.FillColor = fillColor;
            tile.LineWidth = (float)vectorStyle.Outline.Width;
            tile.Path = polygon.ToUIKit(viewport).CGPath;

            return tile;
        }

        public static CALayer RenderMultiPolygonOnLayer(MultiPolygon multiPolygon, IStyle style, IViewport viewport)
        {
            var tile = new CAShapeLayer();

            if (!(style is VectorStyle)) throw new ArgumentException("Style is not of type VectorStyle");
            var vectorStyle = style as VectorStyle;

            var strokeAlpha = (float)vectorStyle.Outline.Color.A / 255;
            var fillAlpha = (float)vectorStyle.Fill.Color.A / 255;

            var strokeColor = new CGColor(new CGColor(vectorStyle.Outline.Color.R, vectorStyle.Outline.Color.G,
                vectorStyle.Outline.Color.B), strokeAlpha);
            var fillColor = new CGColor(new CGColor(vectorStyle.Fill.Color.R, vectorStyle.Fill.Color.G,
                vectorStyle.Fill.Color.B), fillAlpha);

            var path = multiPolygon.ToUIKit(viewport);

            tile.StrokeColor = strokeColor;
            tile.FillColor = fillColor;
            tile.LineWidth = (float)vectorStyle.Outline.Width;
            tile.Path = path.CGPath;

            return tile;
        }

        public static CALayer RenderRasterOnLayer(IRaster raster, IStyle style, IViewport viewport)
        {
            var tile = new CALayer();
            var data = NSData.FromArray(raster.Data.ToArray());
            var image = UIImage.LoadFromData(data);
            var frame = ConvertBoundingBox(raster.GetBoundingBox(), viewport);

            tile.Frame = frame;
            tile.Contents = image.CGImage;

            var aOpacity = new CABasicAnimation
                {
                    KeyPath = @"opacity",
                    From = new NSNumber(0.1),
                    To = new NSNumber(1.0),
                    Duration = 0.6
                };

            tile.AddAnimation(aOpacity, "opacity");

            return tile;
        }

        public static UIImageView RenderRasterOnView(IRaster raster, IStyle style, IViewport viewport)
        {
            var tile = new UIImageView();
            var data = NSData.FromArray(raster.Data.ToArray());
            var image = UIImage.LoadFromData(data);
            var drawRectangle = ConvertBoundingBox(raster.GetBoundingBox(), viewport);

            tile.Image = image;
            tile.Frame = drawRectangle;
            return tile;
        }

        public static UIImage RenderRaster(IRaster raster, IStyle style, IViewport viewport)
        {
            var data = NSData.FromArray(raster.Data.ToArray());
            var image = UIImage.LoadFromData(data);
            var drawRectangle = ConvertBoundingBox(raster.GetBoundingBox(), viewport);
            image.Draw((CGRect)drawRectangle);
            return image;
        }

        public static void PositionRaster(CALayer raster, BoundingBox boundingBox, IViewport viewport)
        {
            var frame = ConvertBoundingBox(boundingBox, viewport);
            raster.Frame = frame;
        }

        public static void PositionPoint(CALayer symbol, CGPoint point, IStyle style, IViewport viewport)
        {
            var frame = ConvertPointBoundingBox(style as SymbolStyle, point.GetBoundingBox(), viewport);
            symbol.Frame = frame;
        }

        public static void PositionMultiPolygon(CALayer shape, MultiPolygon multiPolygon, IStyle style, IViewport viewport)
        {
            var shapeLayer = shape as CAShapeLayer;
            var path = multiPolygon.ToUIKit(viewport);
            //var frame = ConvertBoundingBox (multiPolygon.GetBoundingBox(), viewport);
            shapeLayer.Path = path.CGPath;
            //shape.Frame = frame;
            /*
            if (viewport.Resolution > MinResolution || viewport.Resolution < MaxResolution) {
                //recalculate
                var newImage = RenderMultiPolygonOnLayer (multiPolygon, style, viewport);

                shape.Contents = newImage.Contents;
                shape.Frame = newImage.Frame;

                var resolution = ZoomHelper.ClipToExtremes (Resolutions, viewport.Resolution);

                MinResolution = ZoomHelper.ZoomOut (Resolutions, resolution);
                MaxResolution = ZoomHelper.ZoomIn (Resolutions, resolution);

            } else {
                //reposition Geometry
                var frame = ConvertBoundingBox (multiPolygon.GetBoundingBox(), viewport);
                var newFrame = new RectangleF (frame.X, (frame.Y), frame.Width, frame.Height);

                shape.Frame = newFrame;
                //shape.Frame = frame;
            }
            */
        }

        public static CGRect ConvertPointBoundingBox(SymbolStyle symbolStyle, BoundingBox boundingBox, IViewport viewport)
        {
            var screenMin = viewport.WorldToScreen(boundingBox.Min);
            var screenMax = viewport.WorldToScreen(boundingBox.Max);

            //boundingBox.Offset = symbolStyle.SymbolOffset;
            //var newMin = boundingBox.Min;
            //var newMax = boundingBox.Max;

            if (symbolStyle.SymbolOffset != null)
            {
                screenMin = new Geometries.Point(
                    screenMin.X - symbolStyle.SymbolOffset.X,
                    screenMin.Y - symbolStyle.SymbolOffset.Y);
                screenMax = new Geometries.Point(
                    screenMax.X - symbolStyle.SymbolOffset.X,
                    screenMax.Y - symbolStyle.SymbolOffset.Y);

                var w = viewport.ScreenToWorld(screenMin);

                boundingBox.Offset(new Geometries.Point(w.X - boundingBox.MinX, w.Y - boundingBox.MinY));

                screenMin = viewport.WorldToScreen(boundingBox.Min);
                screenMax = viewport.WorldToScreen(boundingBox.Max);
            }


            var min = new Geometries.Point(screenMin.X - (32 / 2), screenMax.Y - (32 / 2)); //!!!
            var max = new Geometries.Point((min.X + 32), (min.Y + 32)); //!!!

            var frame = RoundToPixel(min, max);
            //if(symbolStyle.SymbolOffset != null)
            //	frame.Offset ((float)symbolStyle.SymbolOffset.X, (float)symbolStyle.SymbolOffset.Y);

            return frame;
        }

        public static CGRect ConvertBoundingBox(BoundingBox boundingBox, IViewport viewport)
        {
            var min = viewport.WorldToScreen(boundingBox.Min);
            var max = viewport.WorldToScreen(boundingBox.Max);

            return RoundToPixel(min, max);
        }

        private static CGRect RoundToPixel(Geometries.Point min, Geometries.Point max)
        {
            // To get seamless aligning you need to round the 
            // corner coordinates to pixel. The new width and
            // height will be a result of that.
            var x = Math.Round(min.X);
            var y = Math.Round(min.Y);
            var width = Math.Round(max.X) - Math.Round(min.X);
            var height = Math.Round(max.Y) - Math.Round(min.Y);

            return new CGRect((float)x, (float)y, (float)width, (float)height);
        }

        private static UIImage GetBitmapCache(IStyle style)
        {
            if (BitmapCache.ContainsKey(style))
                return BitmapCache[style];
            return null;
        }

        private static void SetBitmapCache(IStyle style, UIImage path)
        {
            //caching still needs more work
            if (BitmapCache.Count > 4000) return;
            BitmapCache[style] = path;
        }
    }
}

