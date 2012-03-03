using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using SharpMap;
using SharpMap.Geometries;
using SharpMap.Styles;
using Path = System.Windows.Shapes.Path;
using Point = SharpMap.Geometries.Point;
using Windows = System.Windows.Media;

namespace SilverlightRendering
{
    static class GeometryRenderer
    {
        private static readonly IDictionary<IStyle, BitmapSource> StyleCache = new Dictionary<IStyle, BitmapSource>();
                
        public static UIElement RenderPoint(Point point, IStyle style, IView view)
        {
            if (style is LabelStyle)
            {
                return LabelRenderer.RenderLabel(point, new Offset(), style as LabelStyle, view);
            }

            if (!(style is SymbolStyle)) throw new ArgumentException("Style is not of type SymbolStyle");
            var symbolStyle = style as SymbolStyle;

            FrameworkElement path;
            double width, height;

            if (symbolStyle.UnitType == UnitType.WorldUnit)
            {
                path = ToSymbolPath(symbolStyle);
                width = symbolStyle.Width * symbolStyle.SymbolScale;
                height = symbolStyle.Height * symbolStyle.SymbolScale;                     
            }
            else
            {
                var bitmap = GetCache(style);

                if (bitmap == null)
                {
                    var symbolPath = ToSymbolPath(symbolStyle);
                    bitmap = ToBitmap(symbolPath, symbolPath.Data.Bounds.Width, symbolPath.Data.Bounds.Height);
                    SetCache(style, bitmap);
                }

                var rect = new System.Windows.Shapes.Rectangle();
                rect.Fill = new ImageBrush { ImageSource = bitmap };
                rect.Width = bitmap.PixelWidth * symbolStyle.SymbolScale;
                rect.Height = bitmap.PixelHeight * symbolStyle.SymbolScale;
                path = rect;

                width = bitmap.PixelWidth * symbolStyle.SymbolScale;
                height = bitmap.PixelHeight * symbolStyle.SymbolScale;
            }

            var matrix = new Matrix();
            // flip the image top to bottom:
            MatrixHelper.Translate(ref matrix, - width * 0.5, - height * 0.5);
            MatrixHelper.Invert(ref matrix);
            MatrixHelper.Translate(ref matrix, width * 0.5, height * 0.5);

            MatrixHelper.Translate(ref matrix, 
                point.X + symbolStyle.SymbolOffset.X - width * 0.5,
                point.Y + symbolStyle.SymbolOffset.Y - height * 0.5);
            //for point symbols we want the size to be independent from the resolution. We do this by counter scaling first.
            if (symbolStyle.UnitType != UnitType.WorldUnit)
                MatrixHelper.ScaleAt(ref matrix, view.Resolution, view.Resolution, point.X, point.Y);
            
            MatrixHelper.RotateAt(ref matrix, -symbolStyle.SymbolRotation, point.X, point.Y);
            MatrixHelper.ApplyViewTransform(ref matrix, view);
            path.RenderTransform = new MatrixTransform { Matrix = matrix };
            path.Opacity = symbolStyle.Opacity;
            return path;
        }

        private static Path ToSymbolPath(SymbolStyle symbolStyle)
        {
            var path = new Path();
            double width, height;

            path.StrokeThickness = 0; //The SL default is 1 and causes blurry bitmaps
  
            if (symbolStyle.Symbol == null)
            {
                width = symbolStyle.Width;
                height = symbolStyle.Height;

                if (symbolStyle.Fill != null && symbolStyle.Fill.Color != null)
                    path.Fill = symbolStyle.Fill.Convert();
                else
                    path.Fill = new SolidColorBrush(Colors.Transparent);

                if (symbolStyle.Outline != null)
                {
                    path.Stroke = new SolidColorBrush(symbolStyle.Outline.Color.Convert());
                    path.StrokeThickness = symbolStyle.Outline.Width;
                }
            }
            else
            {
                BitmapImage bitmapImage = CreateBitmapImage(symbolStyle.Symbol.data);
                path.Fill = new ImageBrush { ImageSource = bitmapImage };
                //retrieve width and height from bitmap if set.
                width = bitmapImage.PixelWidth * symbolStyle.SymbolScale;
                height = bitmapImage.PixelHeight * symbolStyle.SymbolScale;
            }

            //set path Data
            if (symbolStyle.SymbolType == SymbolType.Rectangle)
                path.Data = CreateRectangle(width, height, path.StrokeThickness);
            else if (symbolStyle.SymbolType == SymbolType.Ellipse)
                path.Data = CreateEllipse(width, height, path.StrokeThickness);
            return path;
        }

        private static EllipseGeometry CreateEllipse(double width, double height, double strokeThickness)
        {
            var margin = strokeThickness * 0.5;
            var data = new EllipseGeometry();
            data.Center = new System.Windows.Point(width * 0.5 + margin, height * 0.5 + margin);
            data.RadiusX = width * 0.5;
            data.RadiusY = height * 0.5;
            return data;
        }

        private static RectangleGeometry CreateRectangle(double width, double height, double strokeThickness)
        {
            var margin = strokeThickness * 0.5;
                
            var data = new RectangleGeometry
            {
                Rect = new Rect(
                    0 + margin,
                    0 + margin,
                    width + margin,
                    height + margin)
            };
            return data;
        }

        private static BitmapSource ToBitmap(Path element, double width, double height)
        {
#if !SILVERLIGHT
            element.Arrange(new Rect(0, 0, width, height));
            var renderTargetBitmap = new RenderTargetBitmap((int)width, (int)height, 96, 96, new PixelFormat());
            renderTargetBitmap.Render(element);
            return renderTargetBitmap;
#else
            element.UpdateLayout();
            WriteableBitmap bitmap = new WriteableBitmap(element, null);
            bitmap.Invalidate();
            return bitmap;
#endif
        }
#if !WINDOWS_PHONE
        public static DropShadowEffect CreateDropShadow(double angle)
        {
            // Initialize a new DropShadowEffect that will be applied
            // to the Button.
            var myDropShadowEffect = new DropShadowEffect();

            // Set the color of the shadow to Black.
            var myShadowColor = new System.Windows.Media.Color();
            myShadowColor.A = 255; // Note that the alpha value is ignored by Color property. 
            // The Opacity property is used to control the alpha.
            myShadowColor.B = 50;
            myShadowColor.G = 50;
            myShadowColor.R = 50;

            myDropShadowEffect.Color = myShadowColor;

            // Set the direction of where the shadow is cast to 320 degrees.
            myDropShadowEffect.Direction = 45 + angle;

            // Set the depth of the shadow being cast.
            myDropShadowEffect.ShadowDepth = 5;

            // Set the shadow softness to the maximum (range of 0-1).
            myDropShadowEffect.BlurRadius = 6;

            // Set the shadow opacity to half opaque or in other words - half transparent.
            // The range is 0-1.
            myDropShadowEffect.Opacity = 0.5;

            return myDropShadowEffect;
        }
#endif

        private static Path CreatePointPath(SymbolStyle style)
        {
            //todo: use this:
            //style.Symbol.Convert();
            //style.SymbolScale;
            //style.SymbolOffset.Convert();
            //style.SymbolRotation;

            var path = new Path();

            if (style.Symbol == null)
            {
                path.Fill = new SolidColorBrush(Colors.Gray);
            }
            else
            {
                BitmapImage bitmapImage = CreateBitmapImage(style.Symbol.data);

                path.Fill = new ImageBrush { ImageSource = bitmapImage };

                //Changes the rotation of the symbol
                var rotation = new RotateTransform();
                rotation.Angle = style.SymbolRotation;
                rotation.CenterX = bitmapImage.PixelWidth * style.SymbolScale * 0.5;
                rotation.CenterY = bitmapImage.PixelHeight * style.SymbolScale * 0.5;
                path.RenderTransform = rotation;
                //Todo: find a way to get the right values for CenterX en CenterY from the style
            }

            if (style.Outline != null)
            {
                path.Stroke = new SolidColorBrush(style.Outline.Color.Convert());
                path.StrokeThickness = style.Outline.Width;
            }

            return path;
        }

        private static BitmapImage CreateBitmapImage(Stream imageData)
        {
            var bitmapImage = new BitmapImage();
#if !SILVERLIGHT
            imageData.Position = 0;
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = imageData;
            bitmapImage.EndInit();
#else
            imageData.Position = 0;
            bitmapImage.SetSource(imageData);
#endif
            return bitmapImage;
        }

        private static System.Windows.Media.Geometry ConvertSymbol(Point point, SymbolStyle style, IView view)
        {
            Point p = view.WorldToView(point);

            var rect = new RectangleGeometry();
            if (style.Symbol != null)
            {
                var bitmapImage = CreateBitmapImage(style.Symbol.data);
                var width = bitmapImage.PixelWidth * style.SymbolScale;
                var height = bitmapImage.PixelHeight * style.SymbolScale;
                rect.Rect = new Rect(p.X - width * 0.5, p.Y - height * 0.5, width, height);
            }

            return rect;
        }

        public static Path RenderMultiPoint(MultiPoint multiPoint, IStyle style, IView view)
        {
            if (!(style is SymbolStyle)) throw new ArgumentException("Style is not of type SymboStyle");
            var symbolStyle = style as SymbolStyle;
            Path path = CreatePointPath(symbolStyle);
            path.Data = ConvertMultiPoint(multiPoint, symbolStyle, view);
            return path;
        }

        private static GeometryGroup ConvertMultiPoint(MultiPoint multiPoint, SymbolStyle style, IView view)
        {
            var group = new GeometryGroup();
            foreach (Point point in multiPoint)
                group.Children.Add(ConvertSymbol(point, style, view));
            return group;
        }

        public static Path RenderLineString(LineString lineString, IStyle style, IView view)
        {
            if (!(style is VectorStyle)) throw new ArgumentException("Style is not of type VectorStyle");
            var vectorStyle = style as VectorStyle;

            Path path = CreateLineStringPath(vectorStyle);
            path.Data = ConvertLineString(lineString, view);
            return path;
        }

        private static Path CreateLineStringPath(VectorStyle style)
        {
            var path = new Path();
            if (style.Outline != null)
            {
                //todo: render an outline around the line. 
            }
            path.Stroke = new SolidColorBrush(style.Line.Color.Convert());
            path.StrokeThickness = style.Line.Width;
            return path;
        }

        private static Windows.Geometry ConvertLineString(LineString lineString, IView view)
        {
            var pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(CreatePathFigure(lineString, view));
            return pathGeometry;
        }

        private static PathFigure CreatePathFigure(LineString linearRing, IView view)
        {
            var pathFigure = new PathFigure();
            pathFigure.StartPoint = ConvertPoint(linearRing.StartPoint.WorldToMap(view));

            foreach (Point point in linearRing.Vertices)
            {
                pathFigure.Segments.Add(
                    new LineSegment { Point = ConvertPoint(point.WorldToMap(view)) });
            }
            return pathFigure;
        }

        private static System.Windows.Point ConvertPoint(Point point)
        {
            return new System.Windows.Point(point.X, point.Y);
        }

        public static Path RenderMultiLineString(MultiLineString multiLineString, IStyle style, IView view)
        {
            if (!(style is VectorStyle)) throw new ArgumentException("Style is not of type VectorStyle");
            var vectorStyle = style as VectorStyle;
            Path path = CreateLineStringPath(vectorStyle);
            path.Data = ConvertMultiLineString(multiLineString, view);
            return path;
        }

        private static System.Windows.Media.Geometry ConvertMultiLineString(MultiLineString multiLineString, IView view)
        {
            var group = new GeometryGroup();
            foreach (LineString lineString in multiLineString)
                group.Children.Add(ConvertLineString(lineString, view));
            return group;
        }

        public static Path RenderPolygon(Polygon polygon, IStyle style, IView view)
        {
            if (!(style is VectorStyle)) throw new ArgumentException("Style is not of type VectorStyle");
            var vectorStyle = style as VectorStyle;

            Path path = CreatePolygonPath(vectorStyle);
            path.Data = ConvertPolygon(polygon, view);
            return path;
        }

        private static Path CreatePolygonPath(VectorStyle style)
        {
            var path = new Path();
            if (style == null) return path; //!!!
            if (style.Outline != null)
            {
                path.Stroke = new SolidColorBrush(style.Outline.Color.Convert());
                path.StrokeThickness = style.Outline.Width;
            }

            path.Fill = style.Fill.Convert();
            return path;
        }

        private static GeometryGroup ConvertPolygon(Polygon polygon, IView view)
        {
            var group = new GeometryGroup();
            group.FillRule = FillRule.EvenOdd;
            group.Children.Add(ConvertLinearRing(polygon.ExteriorRing, view));
            group.Children.Add(ConvertLinearRings(polygon.InteriorRings, view));
            return group;
        }

        private static PathGeometry ConvertLinearRing(LinearRing linearRing, IView view)
        {
            var pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(CreatePathFigure(linearRing, view));
            return pathGeometry;
        }

        private static PathGeometry ConvertLinearRings(IEnumerable<LinearRing> linearRings, IView view)
        {
            var pathGeometry = new PathGeometry();
            foreach (var linearRing in linearRings)
                pathGeometry.Figures.Add(CreatePathFigure(linearRing, view));
            return pathGeometry;
        }

        public static Path RenderMultiPolygon(MultiPolygon geometry, IStyle style, IView view)
        {
            if (!(style is VectorStyle)) throw new ArgumentException("Style is not of type VectorStyle");
            var vectorStyle = style as VectorStyle;

            Path path = CreatePolygonPath(vectorStyle);
            path.Data = ConvertMultiPolygon(geometry, view);
            return path;
        }

        private static GeometryGroup ConvertMultiPolygon(MultiPolygon geometry, IView view)
        {
            var group = new GeometryGroup();
            foreach (Polygon polygon in geometry.Polygons)
                group.Children.Add(ConvertPolygon(polygon, view));
            return group;
        }

        public static Path RenderRaster(IRaster raster, IStyle style, IView view) 
        {
            Path path = CreateRasterPath(style, raster.Data);
            path.Data = ConvertRaster(raster.GetBoundingBox(), view);

            return path;
        }

        private static Path CreateRasterPath(IStyle style, MemoryStream stream)
        {
            //todo: use this:
            //style.Symbol.Convert();
            //style.SymbolScale;
            //style.SymbolOffset.Convert();
            //style.SymbolRotation;

            var bitmapImage = new BitmapImage();
#if !SILVERLIGHT
            stream.Position = 0;
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
#else
            bitmapImage.SetSource(stream);
#endif
            var path = new Path();
            path.Fill = new ImageBrush { ImageSource = bitmapImage };
            return path;
        }

        private static System.Windows.Media.Geometry ConvertRaster(BoundingBox boundingBox, IView view)
        {
            return new RectangleGeometry
            {
                Rect = RoundToPixel(new Rect(
                    ConvertPoint(view.WorldToView(boundingBox.Min)),
                    ConvertPoint(view.WorldToView(boundingBox.Max))))
            };
        }

        public static Rect RoundToPixel(Rect dest)
        {
            // To get seamless aligning you need to round the 
            // corner coordinates to pixel. The new width and
            // height will be a result of that.
            dest = new Rect(
                Math.Round(dest.Left),
                Math.Round(dest.Top),
                (Math.Round(dest.Right) - Math.Round(dest.Left)),
                (Math.Round(dest.Bottom) - Math.Round(dest.Top)));
            return dest;
        }

        private static BitmapSource GetCache(IStyle style)
        {
            if (StyleCache.ContainsKey(style))
                    return StyleCache[style];
            return null;
        }

        private static void SetCache(IStyle style, BitmapSource path)
        {
            //caching still needs more work
            if (StyleCache.Count > 100) return;
            StyleCache[style] = path;
        }

        public static void PositionRaster(UIElement renderedGeometry, BoundingBox boundingBox, IView view)
        {
            ((RectangleGeometry)((System.Windows.Shapes.Path)renderedGeometry).Data).Rect =
                                     RoundToPixel(new Rect(
                                        ConvertPoint(view.WorldToView(boundingBox.Min)),
                                        ConvertPoint(view.WorldToView(boundingBox.Max))));

        }
    }
}
