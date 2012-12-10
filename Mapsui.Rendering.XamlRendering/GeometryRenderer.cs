using System;
using System.Collections.Generic;
using System.Windows;
#if !NETFX_CORE
using Media = System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Shapes = System.Windows.Shapes;
using WinPoint = System.Windows.Point;
using WinColor = System.Windows.Media.Color;
using WinColors = System.Windows.Media.Colors;
#else
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Media = Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Media3D;
using Windows.UI.Xaml.Media.Imaging;
using Shapes = Windows.UI.Xaml.Shapes;
using WinPoint = Windows.Foundation.Point;
using WinColor = Windows.UI.Color;
using WinColors = Windows.UI.Colors;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
#endif
using Mapsui.Geometries;
using Mapsui.Styles;
using Point = Mapsui.Geometries.Point;

    
namespace Mapsui.Rendering.XamlRendering
{
    static class GeometryRenderer
    {
        private static readonly IDictionary<IStyle, BitmapSource> BitmapCache
            = new Dictionary<IStyle, BitmapSource>();

        public static void PositionPoint(UIElement renderedGeometry, Point point, IStyle style, IViewport viewport)
        {
            var frameworkElement = (FrameworkElement) renderedGeometry;
            var symbolStyle = (style is SymbolStyle) ? style as SymbolStyle : new SymbolStyle();
            
            double width, height;

            if (symbolStyle.UnitType == UnitType.WorldUnit)
            {
                width = symbolStyle.Width*symbolStyle.SymbolScale;
                height = symbolStyle.Height*symbolStyle.SymbolScale;
            }
            else
            {
                width = (renderedGeometry as Shapes.Rectangle).Width;
                height = (renderedGeometry as Shapes.Rectangle).Height;
            }

            var matrix = CreateTransformMatrix(point, viewport, symbolStyle, width, height);
            frameworkElement.RenderTransform = new Media.MatrixTransform { Matrix = matrix };
        }

        public static UIElement RenderPoint(Point point, IStyle style, IViewport viewport)
        {
            var symbolStyle = (style is SymbolStyle) ? style as SymbolStyle : new SymbolStyle();

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
                //hack 
                path = ToSymbolPath(symbolStyle);
                width = 32;
                height = 32;

                //var bitmap = GetBitmapCache(style);

                //if (bitmap == null)
                //{
                //    var symbolPath = ToSymbolPath(symbolStyle);
                //    bitmap = ToBitmap(symbolPath, symbolPath.Data.Bounds.Width, symbolPath.Data.Bounds.Height);
                //    SetBitmapCache(style, bitmap);
                //}

                //var rect = new Shapes.Rectangle();
                //rect.Fill = new Media.ImageBrush { ImageSource = bitmap };
                //rect.Width = bitmap.PixelWidth * symbolStyle.SymbolScale;
                //rect.Height = bitmap.PixelHeight * symbolStyle.SymbolScale;
                //path = rect;

                //width = bitmap.PixelWidth * symbolStyle.SymbolScale;
                //height = bitmap.PixelHeight * symbolStyle.SymbolScale;
            }

            var matrix = CreateTransformMatrix(point, viewport, symbolStyle, width, height);
            //!!!path.RenderTransform = new Media.MatrixTransform { Matrix = matrix };
            path.Opacity = symbolStyle.Opacity;
            return path;
        }

        private static Media.Matrix CreateTransformMatrix(Point point, IViewport viewport, SymbolStyle symbolStyle, double width, double height)
        {
            var matrix = new Media.Matrix();
            // flip the image top to bottom:
            MatrixHelper.Translate(ref matrix, -width * 0.5, -height * 0.5);
            MatrixHelper.Invert(ref matrix);
            MatrixHelper.Translate(ref matrix, width * 0.5, height * 0.5);

            MatrixHelper.Translate(ref matrix,
                point.X + symbolStyle.SymbolOffset.X - width * 0.5,
                point.Y + symbolStyle.SymbolOffset.Y - height * 0.5);
            //for point symbols we want the size to be independent from the resolution. We do this by counter scaling first.
            if (symbolStyle.UnitType != UnitType.WorldUnit)
                MatrixHelper.ScaleAt(ref matrix, viewport.Resolution, viewport.Resolution, point.X, point.Y);

            MatrixHelper.RotateAt(ref matrix, -symbolStyle.SymbolRotation, point.X, point.Y);
            MatrixHelper.ApplyViewTransform(ref matrix, viewport);
            return matrix;
        }

        private static Shapes.Path ToSymbolPath(SymbolStyle symbolStyle)
        {
            var path = new Shapes.Path();
            double width, height;

            path.StrokeThickness = 0; //The SL default is 1 and causes blurry bitmaps
  
            if (symbolStyle.Symbol == null)
            {
                width = symbolStyle.Width;
                height = symbolStyle.Height;

                if (symbolStyle.Fill != null && symbolStyle.Fill.Color != null)
                    path.Fill = symbolStyle.Fill.Convert();
                else
                    path.Fill = new Media.SolidColorBrush(WinColors.Transparent);

                if (symbolStyle.Outline != null)
                {
                    path.Stroke = new Media.SolidColorBrush(symbolStyle.Outline.Color.Convert());
                    path.StrokeThickness = symbolStyle.Outline.Width;
                }
            }
            else
            {
                BitmapImage bitmapImage = CreateBitmapImage(symbolStyle.Symbol.Data);
                path.Fill = new Media.ImageBrush { ImageSource = bitmapImage };
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

        private static Media.EllipseGeometry CreateEllipse(double width, double height, double strokeThickness)
        {
            var margin = strokeThickness * 0.5;
            var data = new Media.EllipseGeometry();
            data.Center = new WinPoint(width * 0.5 + margin, height * 0.5 + margin);
            data.RadiusX = width * 0.5;
            data.RadiusY = height * 0.5;
            return data;
        }

        private static Media.RectangleGeometry CreateRectangle(double width, double height, double strokeThickness)
        {
            var margin = strokeThickness * 0.5;
                
            var data = new Media.RectangleGeometry
            {
                Rect = new Rect(
                    0 + margin,
                    0 + margin,
                    width + margin,
                    height + margin)
            };
            return data;
        }

        private static BitmapSource ToBitmap(Shapes.Path element, double width, double height)
        {
#if NETFX_CORE
            element.UpdateLayout();
            WriteableBitmap bitmap = new WriteableBitmap((int)width, (int)height);
            // and now write a path to the bitmap. I haven't figured out how.
            bitmap.Invalidate();
            return bitmap;
#elif SILVERLIGHT 
            element.UpdateLayout();
            WriteableBitmap bitmap = new WriteableBitmap(element, null);
            bitmap.Invalidate();
            return bitmap;
#else
            element.Arrange(new Rect(0, 0, width, height));
            var renderTargetBitmap = new RenderTargetBitmap((int)width, (int)height, 96, 96, new Media.PixelFormat());
            renderTargetBitmap.Render(element);
            return renderTargetBitmap;
#endif
        }
#if !WINDOWS_PHONE && !NETFX_CORE
        public static DropShadowEffect CreateDropShadow(double angle)
        {
            // Initialize a new DropShadowEffect that will be applied
            // to the Button.
            var myDropShadowEffect = new DropShadowEffect();

            // Set the color of the shadow to Black.
            var myShadowColor = new WinColor();
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

        private static Shapes.Path CreatePointPath(SymbolStyle style)
        {
            //todo: use this:
            //style.Symbol.Convert();
            //style.SymbolScale;
            //style.SymbolOffset.Convert();
            //style.SymbolRotation;

            var path = new Shapes.Path();

            if (style.Symbol == null)
            {
                path.Fill = new Media.SolidColorBrush(WinColors.Gray);
            }
            else
            {
                BitmapImage bitmapImage = CreateBitmapImage(style.Symbol.Data);

                path.Fill = new Media.ImageBrush { ImageSource = bitmapImage };

                //Changes the rotation of the symbol
                var rotation = new Media.RotateTransform();
                rotation.Angle = style.SymbolRotation;
                rotation.CenterX = bitmapImage.PixelWidth * style.SymbolScale * 0.5;
                rotation.CenterY = bitmapImage.PixelHeight * style.SymbolScale * 0.5;
                path.RenderTransform = rotation;
                //Todo: find a way to get the right values for CenterX en CenterY from the style
            }

            if (style.Outline != null)
            {
                path.Stroke = new Media.SolidColorBrush(style.Outline.Color.Convert());
                path.StrokeThickness = style.Outline.Width;
            }
            path.IsHitTestVisible = false;
            return path;
        }

        private static BitmapImage CreateBitmapImage(System.IO.Stream imageData)
        {
            var bitmapImage = new BitmapImage();
#if SILVERLIGHT 
            imageData.Position = 0;
            bitmapImage.SetSource(imageData);     
#elif NETFX_CORE
            imageData.Position = 0;
            var memoryStream = new System.IO.MemoryStream();
            imageData.CopyTo(memoryStream);
            bitmapImage.SetSource(AsyncHelpers.RunSync<IRandomAccessStream>(() =>
                ByteArrayToRandomAccessStream(memoryStream.ToArray())));
#else
            imageData.Position = 0;
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = imageData;
            bitmapImage.EndInit();
#endif
            return bitmapImage;
        }

        private static Media.Geometry ConvertSymbol(Point point, SymbolStyle style, IViewport viewport)
        {
            Point p = viewport.WorldToScreen(point);

            var rect = new Media.RectangleGeometry();
            if (style.Symbol != null)
            {
                var bitmapImage = CreateBitmapImage(style.Symbol.Data);
                var width = bitmapImage.PixelWidth * style.SymbolScale;
                var height = bitmapImage.PixelHeight * style.SymbolScale;
                rect.Rect = new Rect(p.X - width * 0.5, p.Y - height * 0.5, width, height);
            }

            return rect;
        }

        public static Shapes.Path RenderMultiPoint(MultiPoint multiPoint, IStyle style, IViewport viewport)
        {
            if (!(style is SymbolStyle)) throw new ArgumentException("Style is not of type SymboStyle");
            var symbolStyle = style as SymbolStyle;
            Shapes.Path path = CreatePointPath(symbolStyle);
            path.Data = ConvertMultiPoint(multiPoint, symbolStyle, viewport);
            return path;
        }

        private static Media.GeometryGroup ConvertMultiPoint(MultiPoint multiPoint, SymbolStyle style, IViewport viewport)
        {
            var group = new Media.GeometryGroup();
            foreach (Point point in multiPoint)
                group.Children.Add(ConvertSymbol(point, style, viewport));
            return group;
        }

        public static Shapes.Path RenderLineString(LineString lineString, IStyle style, IViewport viewport)
        {
            if (!(style is VectorStyle)) throw new ArgumentException("Style is not of type VectorStyle");
            var vectorStyle = style as VectorStyle;

            Shapes.Path path = CreateLineStringPath(vectorStyle);
            path.Data = ConvertLineString(lineString, viewport);
            return path;
        }

        private static Shapes.Path CreateLineStringPath(VectorStyle style)
        {
            var path = new Shapes.Path();
            if (style.Outline != null)
            {
                //todo: render an outline around the line. 
            }
            path.Stroke = new Media.SolidColorBrush(style.Line.Color.Convert());
            path.StrokeThickness = style.Line.Width;
            path.IsHitTestVisible = false;
            return path;
        }

        private static Media.Geometry ConvertLineString(LineString lineString, IViewport viewport)
        {
            var pathGeometry = new Media.PathGeometry();
            pathGeometry.Figures.Add(CreatePathFigure(lineString, viewport));
            return pathGeometry;
        }

        private static Media.PathFigure CreatePathFigure(LineString linearRing, IViewport viewport)
        {
            var pathFigure = new Media.PathFigure();
            pathFigure.StartPoint = ConvertPoint(WorldToView(linearRing.StartPoint, viewport));

            foreach (Point point in linearRing.Vertices)
            {
                pathFigure.Segments.Add(
                    new Media.LineSegment { Point = ConvertPoint(WorldToView(point, viewport)) });
            }
            return pathFigure;
        }

        public static Point WorldToView(Point point, IViewport viewport)
        {
            return viewport.WorldToScreen(point);
        }

        private static WinPoint ConvertPoint(Point point)
        {
            return new WinPoint(point.X, point.Y);
        }

        public static Shapes.Path RenderMultiLineString(MultiLineString multiLineString, IStyle style, IViewport viewport)
        {
            if (!(style is VectorStyle)) throw new ArgumentException("Style is not of type VectorStyle");
            var vectorStyle = style as VectorStyle;
            Shapes.Path path = CreateLineStringPath(vectorStyle);
            path.Data = ConvertMultiLineString(multiLineString, viewport);
            return path;
        }

        private static Media.Geometry ConvertMultiLineString(MultiLineString multiLineString, IViewport viewport)
        {
            var group = new Media.GeometryGroup();
            foreach (LineString lineString in multiLineString)
                group.Children.Add(ConvertLineString(lineString, viewport));
            return group;
        }

        public static Shapes.Path RenderPolygon(Polygon polygon, IStyle style, IViewport viewport)
        {
            if (!(style is VectorStyle)) throw new ArgumentException("Style is not of type VectorStyle");
            var vectorStyle = style as VectorStyle;

            Shapes.Path path = CreatePolygonPath(vectorStyle);
            path.Data = ConvertPolygon(polygon, viewport);
            return path;
        }

        private static Shapes.Path CreatePolygonPath(VectorStyle style)
        {
            var path = new Shapes.Path();
            if (style == null) return path; //!!!
            if (style.Outline != null)
            {
                path.Stroke = new Media.SolidColorBrush(style.Outline.Color.Convert());
                path.StrokeThickness = style.Outline.Width;
            }

            path.Fill = style.Fill.Convert();
            path.IsHitTestVisible = false;
            return path;
        }

        private static Media.GeometryGroup ConvertPolygon(Polygon polygon, IViewport viewport)
        {
            var group = new Media.GeometryGroup();
            group.FillRule = Media.FillRule.EvenOdd;
            group.Children.Add(ConvertLinearRing(polygon.ExteriorRing, viewport));
            group.Children.Add(ConvertLinearRings(polygon.InteriorRings, viewport));
            return group;
        }

        private static Media.PathGeometry ConvertLinearRing(LinearRing linearRing, IViewport viewport)
        {
            var pathGeometry = new Media.PathGeometry();
            pathGeometry.Figures.Add(CreatePathFigure(linearRing, viewport));
            return pathGeometry;
        }

        private static Media.PathGeometry ConvertLinearRings(IEnumerable<LinearRing> linearRings, IViewport viewport)
        {
            var pathGeometry = new Media.PathGeometry();
            foreach (var linearRing in linearRings)
                pathGeometry.Figures.Add(CreatePathFigure(linearRing, viewport));
            return pathGeometry;
        }

        public static Shapes.Path RenderMultiPolygon(MultiPolygon geometry, IStyle style, IViewport viewport)
        {
            if (!(style is VectorStyle)) throw new ArgumentException("Style is not of type VectorStyle");
            var vectorStyle = style as VectorStyle;

            Shapes.Path path = CreatePolygonPath(vectorStyle);
            path.Data = ConvertMultiPolygon(geometry, viewport);
            return path;
        }

        private static Media.GeometryGroup ConvertMultiPolygon(MultiPolygon geometry, IViewport viewport)
        {
            var group = new Media.GeometryGroup();
            foreach (Polygon polygon in geometry.Polygons)
                group.Children.Add(ConvertPolygon(polygon, viewport));
            return group;
        }

        public static Shapes.Path RenderRaster(IRaster raster, IStyle style, IViewport viewport) 
        {
            Shapes.Path path = CreateRasterPath(style, raster.Data);
            path.Data = ConvertRaster(raster.GetBoundingBox(), viewport);
            MapRenderer.Animate(path, "Opacity", 0, 1, 600, (s, e) => { });
            return path;
        }

        private static Shapes.Path CreateRasterPath(IStyle style, System.IO.MemoryStream stream)
        {
            //todo: use this:
            //style.Symbol.Convert();
            //style.SymbolScale;
            //style.SymbolOffset.Convert();
            //style.SymbolRotation;

            var bitmapImage = new BitmapImage();
#if NETFX_CORE
           stream.Position = 0;
           bitmapImage.SetSource(AsyncHelpers.RunSync<IRandomAccessStream>(() =>
               ByteArrayToRandomAccessStream(stream.ToArray())));
               
#elif !SILVERLIGHT 
            stream.Position = 0;
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
#else
            bitmapImage.SetSource(stream);
#endif
            var path = new Shapes.Path();
            path.Fill = new Media.ImageBrush { ImageSource = bitmapImage };
            path.IsHitTestVisible = false;
            return path;
        }

#if NETFX_CORE

        private static async Task<IRandomAccessStream> ByteArrayToRandomAccessStream(byte[] tile)
        {
            var stream = new InMemoryRandomAccessStream();
            DataWriter dataWriter = new DataWriter(stream);
            dataWriter.WriteBytes(tile);
            await dataWriter.StoreAsync();
            stream.Seek(0);
            return stream;
        }
        
#endif
        private static Media.Geometry ConvertRaster(BoundingBox boundingBox, IViewport viewport)
        {
            return new Media.RectangleGeometry
            {
                Rect = RoundToPixel(new Rect(
                    ConvertPoint(viewport.WorldToScreen(boundingBox.Min)),
                    ConvertPoint(viewport.WorldToScreen(boundingBox.Max))))
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

        private static BitmapSource GetBitmapCache(IStyle style)
        {
            if (BitmapCache.ContainsKey(style))
                    return BitmapCache[style];
            return null;
        }

        private static void SetBitmapCache(IStyle style, BitmapSource path)
        {
            //caching still needs more work
            if (BitmapCache.Count > 4000) return;
            BitmapCache[style] = path;
        }

        public static void PositionRaster(UIElement renderedGeometry, BoundingBox boundingBox, IViewport viewport)
        {
            ((Media.RectangleGeometry)((Shapes.Path)renderedGeometry).Data).Rect =
                                     RoundToPixel(new Rect(
                                        ConvertPoint(viewport.WorldToScreen(boundingBox.Min)),
                                        ConvertPoint(viewport.WorldToScreen(boundingBox.Max))));
        }
    }
}
