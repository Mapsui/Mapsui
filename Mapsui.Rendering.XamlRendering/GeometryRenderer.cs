using System;
using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Styles;
using Point = Mapsui.Geometries.Point;
#if !NETFX_CORE
using System.Windows;
using Media = System.Windows.Media;
using System.Windows.Media.Imaging;
using Shapes = System.Windows.Shapes;
using WinPoint = System.Windows.Point;
using WinColors = System.Windows.Media.Colors;
#else
using Windows.Foundation;
using Windows.UI.Xaml;
using Media = Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Shapes = Windows.UI.Xaml.Shapes;
using WinPoint = Windows.Foundation.Point;
using WinColors = Windows.UI.Colors;
using Windows.Storage.Streams;
using System.Threading.Tasks;
#endif

namespace Mapsui.Rendering.XamlRendering
{
    /// <remarks>In this class are </remarks>
    static class GeometryRenderer
    {
        public static UIElement RenderPoint(Point point, IStyle style, IViewport viewport)
        {
            UIElement symbol;
            var matrix = Media.Matrix.Identity;

            if (style is SymbolStyle)
            {
                var symbolStyle = style as SymbolStyle;

                if (symbolStyle.Symbol == null || symbolStyle.Symbol.Data == null)
                    symbol = CreateSymbolFromVectorStyle(symbolStyle, symbolStyle.Opacity, symbolStyle.SymbolType);
                else
                    symbol = CreateSymbolFromBitmap(symbolStyle.Symbol.Data, symbolStyle.Opacity);
                matrix = CreatePointSymbolMatrix(viewport.Resolution, symbolStyle);
            }
            else
            {
                symbol = CreateSymbolFromVectorStyle((style as VectorStyle) ?? new VectorStyle());
                MatrixHelper.ScaleAt(ref matrix, viewport.Resolution, viewport.Resolution);
            }

            MatrixHelper.Append(ref matrix, CreateTransformMatrix(point, viewport));

            symbol.RenderTransform = new Media.MatrixTransform { Matrix = matrix };

            symbol.IsHitTestVisible = false;

            return symbol;
        }

        private static UIElement CreateSymbolFromVectorStyle(VectorStyle style, double opacity = 1, SymbolType symbolType = SymbolType.Ellipse)
        {
            var path = new Shapes.Path { StrokeThickness = 0 };  //The SL StrokeThickness default is 1 which causes blurry bitmaps

            if (style.Fill != null && style.Fill.Color != null)
                path.Fill = style.Fill.ToXaml();
            else
                path.Fill = new Media.SolidColorBrush(WinColors.Transparent);

            if (style.Outline != null)
            {
                path.Stroke = new Media.SolidColorBrush(style.Outline.Color.ToXaml());
                path.StrokeThickness = style.Outline.Width;
            }

            if (symbolType == SymbolType.Ellipse)
                path.Data = CreateEllipse(SymbolStyle.DefaultWidth, SymbolStyle.DefaultHeight);
            else
                path.Data = CreateRectangle(SymbolStyle.DefaultWidth, SymbolStyle.DefaultHeight);

            path.Opacity = opacity;

            return path;
        }

        private static Media.Matrix CreatePointSymbolMatrix(double resolution, SymbolStyle symbolStyle)
        {
            var matrix = Media.Matrix.Identity;
            MatrixHelper.InvertY(ref matrix);
            var centerX = symbolStyle.SymbolOffset.X;
            var centerY = symbolStyle.SymbolOffset.Y;

            var scale = symbolStyle.SymbolScale;
            MatrixHelper.Translate(ref matrix, centerX, centerY);
            MatrixHelper.ScaleAt(ref matrix, scale, scale);

            //for point symbols we want the size to be independent from the resolution. We do this by counter scaling first.
            if (symbolStyle.UnitType != UnitType.WorldUnit)
                MatrixHelper.ScaleAt(ref matrix, resolution, resolution);
            MatrixHelper.RotateAt(ref matrix, -symbolStyle.SymbolRotation);

            return matrix;
        }

        private static Media.Matrix CreateTransformMatrix(Point point, IViewport viewport)
        {
            var matrix = Media.Matrix.Identity;
            MatrixHelper.Translate(ref matrix, point.X, point.Y);
            var mapCenterX = viewport.Width * 0.5;
            var mapCenterY = viewport.Height * 0.5;

            MatrixHelper.Translate(ref matrix, mapCenterX - viewport.CenterX, mapCenterY - viewport.CenterY);
            MatrixHelper.ScaleAt(ref matrix, 1 / viewport.Resolution, 1 / viewport.Resolution, mapCenterX, mapCenterY);

            // This will invert the Y axis, but will also put images upside down
            MatrixHelper.InvertY(ref matrix, mapCenterY);
            return matrix;
        }

        private static UIElement CreateSymbolFromBitmap(System.IO.Stream data, double opacity)
        {
            var bitmapImage = CreateBitmapImage(data);
            var fill = new Media.ImageBrush { ImageSource = bitmapImage };
            var width = bitmapImage.PixelWidth;
            var height = bitmapImage.PixelHeight;
            
            return new Shapes.Path
                {
                    Data = new Media.RectangleGeometry
                        {
                            Rect = new Rect(-width * 0.5, -height * 0.5, width, height)
                        },
                    Fill = fill,
                    Opacity = opacity
                };
        }

        private static Media.EllipseGeometry CreateEllipse(double width, double height)
        {
            return new Media.EllipseGeometry
                {
                    Center = new WinPoint(0, 0),
                    RadiusX = width * 0.5,
                    RadiusY = height * 0.5
                };
        }

        private static Media.RectangleGeometry CreateRectangle(double width, double height)
        {
            return new Media.RectangleGeometry
                {
                    Rect = new Rect(width * -0.5, height * -0.5,  width, height)
                };
        }

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
                var rotation = new Media.RotateTransform
                    {
                        Angle = style.SymbolRotation,
                        CenterX = bitmapImage.PixelWidth*style.SymbolScale*0.5,
                        CenterY = bitmapImage.PixelHeight*style.SymbolScale*0.5
                    };
                path.RenderTransform = rotation;
            }

            if (style.Outline != null)
            {
                path.Stroke = new Media.SolidColorBrush(style.Outline.Color.ToXaml());
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
            bitmapImage.SetSource(AsyncHelpers.RunSync(() =>
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

        public static UIElement RenderMultiPoint(MultiPoint multiPoint, IStyle style, IViewport viewport)
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

        public static UIElement RenderLineString(LineString lineString, IStyle style, IViewport viewport)
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
            path.Stroke = new Media.SolidColorBrush(style.Line.Color.ToXaml());
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
            var pathFigure = new Media.PathFigure
                {
                    StartPoint = viewport.WorldToScreen(linearRing.StartPoint).ToWinPoint()
                };

            foreach (var point in linearRing.Vertices)
            {
                pathFigure.Segments.Add(
                    new Media.LineSegment { Point = viewport.WorldToScreen(point).ToWinPoint() });
            }
            return pathFigure;
        }
        
        public static UIElement RenderMultiLineString(MultiLineString multiLineString, IStyle style, IViewport viewport)
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

        public static UIElement RenderPolygon(Polygon polygon, IStyle style, IViewport viewport)
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
                path.Stroke = new Media.SolidColorBrush(style.Outline.Color.ToXaml());
                path.StrokeThickness = style.Outline.Width;
            }

            path.Fill = style.Fill.ToXaml();
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
           bitmapImage.SetSource(AsyncHelpers.RunSync(() =>
               ByteArrayToRandomAccessStream(stream.ToArray())));
               
#elif !SILVERLIGHT
            stream.Position = 0;
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
#else
            bitmapImage.SetSource(stream);
#endif
            var path = new Shapes.Path
                {
                    Fill = new Media.ImageBrush {ImageSource = bitmapImage},
                    IsHitTestVisible = false
                };
            return path;
        }

#if NETFX_CORE

        private static async Task<IRandomAccessStream> ByteArrayToRandomAccessStream(byte[] tile)
        {
            var stream = new InMemoryRandomAccessStream();
            var dataWriter = new DataWriter(stream);
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
                    viewport.WorldToScreen(boundingBox.Min).ToWinPoint(),
                    viewport.WorldToScreen(boundingBox.Max).ToWinPoint()))
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

        public static void PositionPoint(UIElement renderedGeometry, Point point, IStyle style, IViewport viewport)
        {
            var matrix = Media.Matrix.Identity;
            if (style is SymbolStyle) matrix = CreatePointSymbolMatrix(viewport.Resolution, style as SymbolStyle);
            else MatrixHelper.ScaleAt(ref matrix, viewport.Resolution, viewport.Resolution);
            MatrixHelper.Append(ref matrix, CreateTransformMatrix(point, viewport));
            renderedGeometry.RenderTransform = new Media.MatrixTransform { Matrix = matrix };
        }

        public static void PositionRaster(UIElement renderedGeometry, BoundingBox boundingBox, IViewport viewport)
        {
            ((Media.RectangleGeometry)((Shapes.Path)renderedGeometry).Data).Rect =
                RoundToPixel(new Rect(
                viewport.WorldToScreen(boundingBox.Min).ToWinPoint(),
                viewport.WorldToScreen(boundingBox.Max).ToWinPoint()));
        }

        public static void PositionGeometry(UIElement renderedGeometry, IStyle style, IViewport viewport)
        {

        }
    }
}
