using System;
using System.IO;
using Mapsui.Geometries;
using Mapsui.Styles;
using Point = Mapsui.Geometries.Point;
#if !NETFX_CORE
using System.Windows;
using System.Windows.Media.Imaging;
using XamlMedia = System.Windows.Media;
using XamlShapes = System.Windows.Shapes;
using XamlPoint = System.Windows.Point;
using XamlColors = System.Windows.Media.Colors;
#else
using Windows.Foundation;
using Windows.UI.Xaml;
using XamlMedia = Windows.UI.Xaml.Media;
using XamlShapes = Windows.UI.Xaml.Shapes;
using XamlPoint = Windows.Foundation.Point;
using XamlColors = Windows.UI.Colors;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
#endif

namespace Mapsui.Rendering.Xaml
{
    ///<remarks>
    /// In this class there are a lot of collistions in class names between Mapsui
    /// and the .net framework libaries I use for Xaml rendering. I resolve this by using
    /// namespace aliases. I will use 'Xaml' in namespace and method names to refer to 
    /// all .net framework classes related to Xaml, even if they are not.
    /// </remarks>
    static class GeometryRenderer
    {
        public static UIElement RenderPoint(Point point, IStyle style, IViewport viewport)
        {
            UIElement symbol;
            var matrix = XamlMedia.Matrix.Identity;

            if (style is SymbolStyle)
            {
                var symbolStyle = style as SymbolStyle;

                if (symbolStyle.ResourceId < 0)
                    symbol = CreateSymbolFromVectorStyle(symbolStyle, symbolStyle.Opacity, symbolStyle.SymbolType);
                else
                    symbol = CreateSymbolFromBitmap(BitmapRegistry.Instance.Get(symbolStyle.ResourceId), symbolStyle.Opacity);
                matrix = CreatePointSymbolMatrix(viewport.Resolution, symbolStyle);
            }
            else
            {
                symbol = CreateSymbolFromVectorStyle((style as VectorStyle) ?? new VectorStyle());
                MatrixHelper.ScaleAt(ref matrix, viewport.Resolution, viewport.Resolution);
            }

            MatrixHelper.Append(ref matrix, CreateTransformMatrix(point, viewport));

            symbol.RenderTransform = new XamlMedia.MatrixTransform { Matrix = matrix };

            symbol.IsHitTestVisible = false;

            return symbol;
        }

        private static UIElement CreateSymbolFromVectorStyle(VectorStyle style, double opacity = 1, SymbolType symbolType = SymbolType.Ellipse)
        {
            var path = new XamlShapes.Path { StrokeThickness = 0 };  //The SL StrokeThickness default is 1 which causes blurry bitmaps

            if (style.Fill != null && style.Fill.Color != null)
                path.Fill = style.Fill.ToXaml();
            else
                path.Fill = new XamlMedia.SolidColorBrush(XamlColors.Transparent);

            if (style.Outline != null)
            {
                path.Stroke = new XamlMedia.SolidColorBrush(style.Outline.Color.ToXaml());
                path.StrokeThickness = style.Outline.Width;
            }

            if (symbolType == SymbolType.Ellipse)
                path.Data = CreateEllipse(SymbolStyle.DefaultWidth, SymbolStyle.DefaultHeight);
            else
                path.Data = CreateRectangle(SymbolStyle.DefaultWidth, SymbolStyle.DefaultHeight);

            path.Opacity = opacity;

            return path;
        }

        private static XamlMedia.Matrix CreatePointSymbolMatrix(double resolution, SymbolStyle symbolStyle)
        {
            var matrix = XamlMedia.Matrix.Identity;
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

        private static XamlMedia.Matrix CreateTransformMatrix(Point point, IViewport viewport)
        {
            var matrix = XamlMedia.Matrix.Identity;
            MatrixHelper.Translate(ref matrix, point.X, point.Y);
            var mapCenterX = viewport.Width * 0.5;
            var mapCenterY = viewport.Height * 0.5;

            MatrixHelper.Translate(ref matrix, mapCenterX - viewport.Center.X, mapCenterY - viewport.Center.Y);
            MatrixHelper.ScaleAt(ref matrix, 1 / viewport.Resolution, 1 / viewport.Resolution, mapCenterX, mapCenterY);

            // This will invert the Y axis, but will also put images upside down
            MatrixHelper.InvertY(ref matrix, mapCenterY);
            return matrix;
        }

        private static XamlMedia.Matrix CreateTransformMatrix1(IViewport viewport)
        {
            var matrix = XamlMedia.Matrix.Identity;
            var mapCenterX = viewport.Width * 0.5;
            var mapCenterY = viewport.Height * 0.5;

            MatrixHelper.Translate(ref matrix, mapCenterX - viewport.Center.X, mapCenterY - viewport.Center.Y);
            MatrixHelper.ScaleAt(ref matrix, 1 / viewport.Resolution, 1 / viewport.Resolution, mapCenterX, mapCenterY);

            // This will invert the Y axis, but will also put images upside down
            MatrixHelper.InvertY(ref matrix, mapCenterY);
            return matrix;
        }

        private static UIElement CreateSymbolFromBitmap(Stream data, double opacity)
        {
            var bitmapImage = CreateBitmapImage(data);
            var fill = new XamlMedia.ImageBrush { ImageSource = bitmapImage };
            var width = bitmapImage.PixelWidth;
            var height = bitmapImage.PixelHeight;

            return new XamlShapes.Path
            {
                Data = new XamlMedia.RectangleGeometry
                {
                    Rect = new Rect(-width * 0.5, -height * 0.5, width, height)
                },
                Fill = fill,
                Opacity = opacity
            };
        }

        private static XamlMedia.EllipseGeometry CreateEllipse(double width, double height)
        {
            return new XamlMedia.EllipseGeometry
            {
                Center = new XamlPoint(0, 0),
                RadiusX = width * 0.5,
                RadiusY = height * 0.5
            };
        }

        private static XamlMedia.RectangleGeometry CreateRectangle(double width, double height)
        {
            return new XamlMedia.RectangleGeometry
            {
                Rect = new Rect(width * -0.5, height * -0.5, width, height)
            };
        }

        private static XamlShapes.Path CreatePointPath(SymbolStyle style)
        {
            //todo: use this:
            //style.Symbol.Convert();
            //style.SymbolScale;
            //style.SymbolOffset.Convert();
            //style.SymbolRotation;

            var path = new XamlShapes.Path();

            if (style.Symbol == null)
            {
                path.Fill = new XamlMedia.SolidColorBrush(XamlColors.Gray);
            }
            else
            {
                BitmapImage bitmapImage = CreateBitmapImage(BitmapRegistry.Instance.Get(style.ResourceId));

                path.Fill = new XamlMedia.ImageBrush { ImageSource = bitmapImage };

                //Changes the rotation of the symbol
                var rotation = new XamlMedia.RotateTransform
                {
                    Angle = style.SymbolRotation,
                    CenterX = bitmapImage.PixelWidth * style.SymbolScale * 0.5,
                    CenterY = bitmapImage.PixelHeight * style.SymbolScale * 0.5
                };
                path.RenderTransform = rotation;
            }

            if (style.Outline != null)
            {
                path.Stroke = new XamlMedia.SolidColorBrush(style.Outline.Color.ToXaml());
                path.StrokeThickness = style.Outline.Width;
            }
            path.IsHitTestVisible = false;
            return path;
        }

        private static BitmapImage CreateBitmapImage(Stream imageData)
        {
            var bitmapImage = new BitmapImage();
#if SILVERLIGHT
            imageData.Position = 0;
            bitmapImage.SetSource(imageData);
#elif NETFX_CORE
            imageData.Position = 0;
            var memoryStream = new System.IO.MemoryStream();
            imageData.CopyTo(memoryStream);
            bitmapImage.SetSource(ByteArrayToRandomAccessStream(memoryStream.ToArray()).Result);
#else
            imageData.Position = 0;
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = imageData;
            bitmapImage.EndInit();
#endif
            return bitmapImage;
        }

        private static XamlMedia.Geometry ConvertSymbol(Point point, SymbolStyle style, IViewport viewport)
        {
            Point p = viewport.WorldToScreen(point);

            var rect = new XamlMedia.RectangleGeometry();
            if (style.Symbol != null)
            {
                var bitmapImage = CreateBitmapImage(BitmapRegistry.Instance.Get(style.ResourceId));
                var width = bitmapImage.PixelWidth * style.SymbolScale;
                var height = bitmapImage.PixelHeight * style.SymbolScale;
                rect.Rect = new Rect(p.X - width * 0.5, p.Y - height * 0.5, width, height);
            }

            return rect;
        }

        public static UIElement RenderMultiPoint(MultiPoint multiPoint, IStyle style, IViewport viewport)
        {
            // This method needs a test
            if (!(style is SymbolStyle)) throw new ArgumentException("Style is not of type SymboStyle");
            var symbolStyle = style as SymbolStyle;
            XamlShapes.Path path = CreatePointPath(symbolStyle);
            path.Data = ConvertMultiPoint(multiPoint, symbolStyle, viewport);
            path.RenderTransform = new XamlMedia.MatrixTransform { Matrix = CreateTransformMatrix1(viewport) };
            return path;
        }

        private static XamlMedia.GeometryGroup ConvertMultiPoint(MultiPoint multiPoint, SymbolStyle style, IViewport viewport)
        {
            var group = new XamlMedia.GeometryGroup();
            foreach (Point point in multiPoint)
                group.Children.Add(ConvertSymbol(point, style, viewport));
            return group;
        }

        public static UIElement RenderLineString(LineString lineString, IStyle style, IViewport viewport)
        {
            if (!(style is VectorStyle)) throw new ArgumentException("Style is not of type VectorStyle");
            var vectorStyle = style as VectorStyle;

            XamlShapes.Path path = CreateLineStringPath(vectorStyle);
            path.Data = lineString.ToXaml();
            path.RenderTransform = new XamlMedia.MatrixTransform { Matrix = CreateTransformMatrix1(viewport) };
            CounterScaleLineWidth(path, viewport.Resolution);
            return path;
        }

        private static XamlShapes.Path CreateLineStringPath(VectorStyle style)
        {
            var path = new XamlShapes.Path();
            if (style.Outline != null)
            {
                //todo: render an outline around the line. 
            }
            path.Stroke = new XamlMedia.SolidColorBrush(style.Line.Color.ToXaml());
            path.Tag = style.Line.Width; // see #linewidthhack
            path.IsHitTestVisible = false;
            return path;
        }

        public static UIElement RenderMultiLineString(MultiLineString multiLineString, IStyle style, IViewport viewport)
        {
            if (!(style is VectorStyle)) throw new ArgumentException("Style is not of type VectorStyle");
            var vectorStyle = style as VectorStyle;

            XamlShapes.Path path = CreateLineStringPath(vectorStyle);
            path.Data = multiLineString.ToXaml();
            path.RenderTransform = new XamlMedia.MatrixTransform { Matrix = CreateTransformMatrix1(viewport) };
            CounterScaleLineWidth(path, viewport.Resolution);
            return path;
        }

        public static UIElement RenderPolygon(Polygon polygon, IStyle style, IViewport viewport)
        {
            if (!(style is VectorStyle)) throw new ArgumentException("Style is not of type VectorStyle");
            var vectorStyle = style as VectorStyle;

            XamlShapes.Path path = CreatePolygonPath(vectorStyle, viewport.Resolution);
            path.Data = polygon.ToXaml();
            path.RenderTransform = new XamlMedia.MatrixTransform { Matrix = CreateTransformMatrix1(viewport) };
            path.UseLayoutRounding = true;
            return path;
        }

        private static XamlShapes.Path CreatePolygonPath(VectorStyle style, double resolution)
        {
            var path = new XamlShapes.Path();
            if (style.Outline != null)
            {
                path.Stroke = new XamlMedia.SolidColorBrush(style.Outline.Color.ToXaml());
                path.StrokeThickness = style.Outline.Width * resolution;
                path.Tag = style.Outline.Width; // see #linewidthhack
            }
            path.Fill = style.Fill.ToXaml();
            path.IsHitTestVisible = false;
            return path;
        }

        public static XamlShapes.Path RenderMultiPolygon(MultiPolygon geometry, IStyle style, IViewport viewport)
        {
            if (!(style is VectorStyle)) throw new ArgumentException("Style is not of type VectorStyle");
            var vectorStyle = style as VectorStyle;
            var path = CreatePolygonPath(vectorStyle, viewport.Resolution);
            path.Data = geometry.ToXaml();
            path.RenderTransform = new XamlMedia.MatrixTransform { Matrix = CreateTransformMatrix1(viewport) };
            return path;
        }

        public static XamlShapes.Path RenderRaster(IRaster raster, IStyle style, IViewport viewport)
        {
            var path = CreateRasterPath(style, raster.Data);
            path.Data = ConvertRaster(raster.GetBoundingBox(), viewport);

            // path.Stroke = new XamlMedia.SolidColorBrush(XamlColors.Red);
            // path.StrokeThickness = 6;

            return path;
        }

        private static XamlShapes.Path CreateRasterPath(IStyle style, MemoryStream stream)
        {
            //todo: use this:
            //style.Symbol.Convert();
            //style.SymbolScale;
            //style.SymbolOffset.Convert();
            //style.SymbolRotation;

            var bitmapImage = new BitmapImage();
#if NETFX_CORE
            stream.Position = 0;
            bitmapImage.SetSource(ByteArrayToRandomAccessStream(stream.ToArray()).Result);

#elif !SILVERLIGHT
            var localStream = new MemoryStream();
            stream.Position = 0;
            stream.CopyTo(localStream);
            localStream.Position = 0;
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = localStream;
            bitmapImage.EndInit();
#else
            bitmapImage.SetSource(stream);
#endif
            var path = new XamlShapes.Path
            {
                Fill = new XamlMedia.ImageBrush { ImageSource = bitmapImage },
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
        private static XamlMedia.Geometry ConvertRaster(BoundingBox boundingBox, IViewport viewport)
        {
            return new XamlMedia.RectangleGeometry
            {
                Rect = RoundToPixel(new Rect(
                    viewport.WorldToScreen(boundingBox.Min).ToXaml(),
                    viewport.WorldToScreen(boundingBox.Max).ToXaml()))
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
            var matrix = XamlMedia.Matrix.Identity;
            if (style is SymbolStyle) matrix = CreatePointSymbolMatrix(viewport.Resolution, style as SymbolStyle);
            else MatrixHelper.ScaleAt(ref matrix, viewport.Resolution, viewport.Resolution);
            MatrixHelper.Append(ref matrix, CreateTransformMatrix(point, viewport));
            renderedGeometry.RenderTransform = new XamlMedia.MatrixTransform { Matrix = matrix };
        }

        public static void PositionRaster(UIElement renderedGeometry, BoundingBox boundingBox, IViewport viewport)
        {
            ((XamlMedia.RectangleGeometry)((XamlShapes.Path)renderedGeometry).Data).Rect =
                RoundToPixel(new Rect(
                viewport.WorldToScreen(boundingBox.Min).ToXaml(),
                viewport.WorldToScreen(boundingBox.Max).ToXaml()));
        }

        public static void PositionGeometry(UIElement renderedGeometry, IViewport viewport)
        {
            CounterScaleLineWidth(renderedGeometry, viewport.Resolution);
            renderedGeometry.RenderTransform = new XamlMedia.MatrixTransform { Matrix = CreateTransformMatrix1(viewport) };
        }

        private static void CounterScaleLineWidth(UIElement renderedGeometry, double resolution)
        {
            // #linewidthhack
            // When the RenderTransform Matrix is applied the width of the line
            // is scaled along with the rest. We want the outline to have a fixed
            // width independent of the scale. So here we counter scale using
            // the orginal width stored in the Tag.
            if (renderedGeometry is XamlShapes.Path)
            {
                var path = renderedGeometry as XamlShapes.Path;
                if (path.Tag is double?)
                    path.StrokeThickness = (path.Tag as double?).Value * resolution;
            }
        }
    }
}
