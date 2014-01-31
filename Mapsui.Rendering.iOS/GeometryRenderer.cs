using Mapsui.Geometries;
using Mapsui.Styles;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Mapsui.Rendering.iOS
{
    static class GeometryRenderer
    {
        //public static List<double> Resolutions;
        //public static double MinResolution;
        //public static double MaxResolution;

        private static readonly IDictionary<IStyle, UIImage> BitmapCache
            = new Dictionary<IStyle, UIImage>();

        public static CALayer RenderPoint(Geometries.Point point, IStyle style, IViewport viewport)
        {
            var tile = new CALayer();
            var rotation = 0.0;

            if (style is SymbolStyle)
            {
                var symbolStyle = style as SymbolStyle;
                var frame = ConvertPointBoundingBox(symbolStyle, point.GetBoundingBox(), viewport);

                if (symbolStyle.Symbol == null || symbolStyle.Symbol.Data == null)
                {
                    tile = CreateSymbolFromVectorStyle(symbolStyle, frame, symbolStyle.Opacity, symbolStyle.SymbolType);
                }
                else
                {

                    tile = CreateSymbolFromBitmap(symbolStyle, frame, symbolStyle.Opacity);
                }

                rotation = symbolStyle.SymbolRotation;
                //matrix = CreatePointSymbolMatrix(viewport.Resolution, symbolStyle);
                if (symbolStyle.Outline != null)
                {

                    float strokeAlpha = (float)symbolStyle.Outline.Color.A / 255;

                    tile.BorderColor = new CGColor(new CGColor(symbolStyle.Outline.Color.R, symbolStyle.Outline.Color.G,
                                                                symbolStyle.Outline.Color.B), strokeAlpha);
                    tile.BorderWidth = (float)symbolStyle.Outline.Width;
                }
            }
            else
            {
                //var frame = ConvertPointBoundingBox(symbolStyle, point.GetBoundingBox(), viewport);
                //tile = CreateSymbolFromVectorStyle((style as VectorStyle) ?? new VectorStyle());
                //MatrixHelper.ScaleAt(ref matrix, viewport.Resolution, viewport.Resolution);
            }

            //var symbolStyle = style as SymbolStyle;
            //var frame = ConvertPointBoundingBox(symbolStyle, point.GetBoundingBox(), viewport);
            /*
            var image = CreateSymbolFromBitmap (symbolStyle);

            tile.Contents = image.CGImage;
            tile.Frame = frame;
            */


            var radians = Math.PI * rotation / 180.0;
            var aOpacity = new CABasicAnimation
                {
                    KeyPath = @"transform.rotation.z",
                    From = new NSNumber(radians),
                    To = new NSNumber(radians),
                    Duration = 0.1,
                    RemovedOnCompletion = false,
                    FillMode = CAFillMode.Forwards
                };

            //aOpacity.From = new NSNumber(0);//new NSNumber(0.1);

            tile.AddAnimation(aOpacity, "transform.rotation.z");
            tile.ContentsScale = 0.1f;

            return tile;
        }

        private static CALayer CreateSymbolFromVectorStyle(VectorStyle style, RectangleF frame, double opacity = 1, SymbolType symbolType = SymbolType.Ellipse)
        {
            var symbol = new CAShapeLayer();

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

            //symbol.Frame = frame;
            var path = UIBezierPath.FromRoundedRect(new RectangleF(0, 0, frame.Width, frame.Height), frame.Width / 2);
            symbol.Path = path.CGPath;

            if (symbolType == SymbolType.Rectangle)
            {
            }
            else
            {
            }

            symbol.Opacity = (float)opacity;

            return symbol;
        }

        public static CALayer RenderPolygonOnLayer(Polygon polygon, IStyle style, IViewport viewport)
        {
            var tile = new CAShapeLayer();

            if (!(style is VectorStyle)) throw new ArgumentException("Style is not of type VectorStyle");
            var vectorStyle = style as VectorStyle;

            float strokeAlpha = (float)vectorStyle.Outline.Color.A / 255;
            float fillAlpha = (float)vectorStyle.Fill.Color.A / 255;

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

            float strokeAlpha = (float)vectorStyle.Outline.Color.A / 255;
            float fillAlpha = (float)vectorStyle.Fill.Color.A / 255;

            var strokeColor = new CGColor(new CGColor(vectorStyle.Outline.Color.R, vectorStyle.Outline.Color.G,
                                                      vectorStyle.Outline.Color.B), strokeAlpha);
            var fillColor = new CGColor(new CGColor(vectorStyle.Fill.Color.R, vectorStyle.Fill.Color.G,
                                                    vectorStyle.Fill.Color.B), fillAlpha);

            /*
            var bbRect = GeometryRenderer.ConvertBoundingBox (multiPolygon.GetBoundingBox(), viewport);
            var offset = new System.Drawing.Point ((int)bbRect.GetMinX(),
                                                   (int)bbRect.GetMinY());

            GeometryExtension.OffSet = offset;
            */

            var path = GeometryExtension.ToUIKit(multiPolygon, viewport);

            tile.StrokeColor = strokeColor;
            tile.FillColor = fillColor;
            tile.LineWidth = (float)vectorStyle.Outline.Width;
            tile.Path = path.CGPath;

            return tile;
        }

        private static CALayer CreateSymbolFromBitmap(SymbolStyle style, RectangleF frame, double opacity)
        {
            var tile = new CALayer();

            var stream = (MemoryStream)style.Symbol.Data;
            var data = NSData.FromArray(stream.ToArray());
            var image = UIImage.LoadFromData(data);

            tile.Contents = image.CGImage;
            tile.Frame = frame;
            tile.Opacity = (float)opacity;

            return tile;
        }

        private static UIImage RenderSymbolFromVectorStyle(SymbolStyle symbolStyle, RectangleF drawRect)
        {
            var context = UIGraphics.GetCurrentContext();

            UIGraphics.PushContext(context);

            var strokeColor = new CGColor(symbolStyle.Outline.Color.R, symbolStyle.Outline.Color.G, symbolStyle.Outline.Color.B);
            var fillColor = new CGColor(symbolStyle.Fill.Color.R, symbolStyle.Fill.Color.G, symbolStyle.Fill.Color.B);

            context.SetStrokeColor(strokeColor);
            context.SetFillColor(fillColor);
            context.StrokeRect(drawRect);

            UIGraphics.PopContext();
            var retImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            return retImage;
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
            /*
            var anim = new CABasicAnimation();
            anim.KeyPath = @"opacity";
            anim.From = new NSNumber(0.1);
            anim.To = new NSNumber(1.0);
            anim.Duration = 0.6;

            tile.AddAnimation(anim, "opacity");
            */
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

            /*
            Console.WriteLine ("RenderOnView minx: " + drawRectangle.GetMinX() + " miny: " + drawRectangle.GetMinY() +
                               " maxx: " + drawRectangle.GetMaxX()+ " maxy: " + drawRectangle.GetMaxY() + 
                               " width: " + drawRectangle.Width + " heigth: " + drawRectangle.Height);
                               */

            return tile;
        }

        public static UIImage RenderRaster(IRaster raster, IStyle style, IViewport viewport)
        {
            var data = NSData.FromArray(raster.Data.ToArray());
            var image = UIImage.LoadFromData(data);
            var drawRectangle = ConvertBoundingBox(raster.GetBoundingBox(), viewport);

            /*
            Console.WriteLine ("RenderRaster minx: " + drawRectangle.GetMinX() + " miny: " + drawRectangle.GetMinY() +
                               " maxx: " + drawRectangle.GetMaxX()+ " maxy: " + drawRectangle.GetMaxY() + 
                               " width: " + drawRectangle.Width + " heigth: " + drawRectangle.Height);
                               */

            image.Draw(drawRectangle);

            return image;
        }

        public static void PositionRaster(CALayer raster, BoundingBox boundingBox, IViewport viewport)
        {
            var frame = ConvertBoundingBox(boundingBox, viewport);
            raster.Frame = frame;
        }

        public static void PositionPoint(CALayer symbol, Geometries.Point point, IStyle style, IViewport viewport)
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

        public static RectangleF ConvertPointBoundingBox(SymbolStyle symbolStyle, BoundingBox boundingBox, IViewport viewport)
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

        public static RectangleF ConvertBoundingBox(BoundingBox boundingBox, IViewport viewport)
        {
            var min = viewport.WorldToScreen(boundingBox.Min);
            var max = viewport.WorldToScreen(boundingBox.Max);

            return RoundToPixel(min, max);
        }

        private static RectangleF RoundToPixel(Geometries.Point min, Geometries.Point max)
        {
            // To get seamless aligning you need to round the 
            // corner coordinates to pixel. The new width and
            // height will be a result of that.
            var x = Math.Round(min.X);
            var y = Math.Round(min.Y);
            var width = Math.Round(max.X) - Math.Round(min.X);
            var height = Math.Round(max.Y) - Math.Round(min.Y);

            return new RectangleF((float)x, (float)y, (float)width, (float)height);
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

        private static System.Drawing.PointF ConvertPoint(Mapsui.Geometries.Point point)
        {
            return new System.Drawing.PointF((float)point.X, (float)point.Y);
        }
    }
}

