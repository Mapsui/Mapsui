using System;
using System.IO;
using Mapsui.Geometries;
using Mapsui.Styles;
using Point = Mapsui.Geometries.Point;
using System.Windows;
using System.Windows.Media.Imaging;
using XamlMedia = System.Windows.Media;
using XamlShapes = System.Windows.Shapes;
using XamlColors = System.Windows.Media.Colors;

namespace Mapsui.Rendering.Xaml
{
    // Note: In this class there are a lot of collisions in namespaces between Mapsui
    // and the .NET framework libaries I use for Xaml rendering. I resolve this by using
    // namespace aliases. I will use 'Xaml' in namespace and method names to refer to 
    // all .net framework classes related to Xaml, even if they are not.
    public static class GeometryRenderer
    {
        public static XamlMedia.Matrix CreateTransformMatrix(IReadOnlyViewport viewport, Point point)
        {
            var pointOffsetFromViewPortCenterX = point.X - viewport.Center.X;
            var pointOffsetFromViewPortCenterY = point.Y - viewport.Center.Y;
            var mapCenterX = viewport.Width * 0.5;
            var mapCenterY = viewport.Height * 0.5;

            var matrix = XamlMedia.Matrix.Identity;
            MatrixHelper.Translate(ref matrix, pointOffsetFromViewPortCenterX, pointOffsetFromViewPortCenterY);
            if (viewport.IsRotated) { MatrixHelper.Rotate(ref matrix, -viewport.Rotation); }
            MatrixHelper.Translate(ref matrix, mapCenterX, mapCenterY);
            MatrixHelper.ScaleAt(ref matrix, 1 / viewport.Resolution, 1 / viewport.Resolution, mapCenterX, mapCenterY);
            MatrixHelper.InvertY(ref matrix, mapCenterY); // To invert Y axis, but also puts images upside down
            return matrix;
        }
        
        public static XamlMedia.Matrix CreateTransformMatrix(IReadOnlyViewport viewport)
        {
            return CreateTransformMatrix(viewport, new Point(0, 0));
        }

        private static XamlShapes.Path CreatePointPath(SymbolStyle style)
        {
            // todo: use this:
            // style.Symbol.Convert();
            // style.SymbolScale;
            // style.SymbolOffset.Convert();
            // style.SymbolRotation;

            var path = new XamlShapes.Path { Opacity = style.Opacity };

            if (style.BitmapId < 0)
            {
                path.Fill = new XamlMedia.SolidColorBrush(XamlColors.Gray);
            }
            else
            {
                BitmapImage bitmapImage = ((Stream)BitmapRegistry.Instance.Get(style.BitmapId)).ToBitmapImage();

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

        private static XamlMedia.Geometry ConvertSymbol(Point point, SymbolStyle style, IReadOnlyViewport viewport)
        {
            Point p = viewport.WorldToScreen(point);

            var rect = new XamlMedia.RectangleGeometry();
            if (style.BitmapId >= 0)
            {
                var bitmapImage = ((Stream)BitmapRegistry.Instance.Get(style.BitmapId)).ToBitmapImage();
                var width = bitmapImage.PixelWidth * style.SymbolScale;
                var height = bitmapImage.PixelHeight * style.SymbolScale;
                rect.Rect = new Rect(p.X - width * 0.5, p.Y - height * 0.5, width, height);
            }

            return rect;
        }

        public static XamlShapes.Shape RenderMultiPoint(MultiPoint multiPoint, IStyle style, IReadOnlyViewport viewport,
            SymbolCache symbolCache)
        {
            // This method needs a test
            // This methode should re-use PointRenderer
            if (!(style is SymbolStyle)) throw new ArgumentException("Style is not of type SymboStyle");
            var symbolStyle = style as SymbolStyle;
            var path = CreatePointPath(symbolStyle);
            path.Data = ConvertMultiPoint(multiPoint, symbolStyle, viewport);
            path.RenderTransform = new XamlMedia.MatrixTransform { Matrix = CreateTransformMatrix(viewport) };
            return path;
        }

        private static XamlMedia.GeometryGroup ConvertMultiPoint(MultiPoint multiPoint, SymbolStyle style, IReadOnlyViewport viewport)
        {
            var group = new XamlMedia.GeometryGroup();
            foreach (var geometry in multiPoint)
            {
                var point = (Point)geometry;
                group.Children.Add(ConvertSymbol(point, style, viewport));
            }
            return group;
        }

        public static XamlShapes.Path RenderRaster(IRaster raster, IStyle style, IReadOnlyViewport viewport)
        {
            var path = CreateRasterPath(style, raster.Data);
            path.Data = new XamlMedia.RectangleGeometry();
            PositionRaster(path, raster.BoundingBox, viewport);
            return path;
        }

        private static XamlShapes.Path CreateRasterPath(IStyle style, MemoryStream stream)
        {
            var bitmapImage = new BitmapImage();

            var localStream = new MemoryStream();
            stream.Position = 0;
            stream.CopyTo(localStream);
            localStream.Position = 0;
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = localStream;
            bitmapImage.EndInit();

            var path = new XamlShapes.Path
            {
                Fill = new XamlMedia.ImageBrush { ImageSource = bitmapImage },
                IsHitTestVisible = false,
                SnapsToDevicePixels = true

            };

            if (Utilities.DeveloperTools.DeveloperMode)
            {
                var color = (style as VectorStyle)?.Line.Color.ToXaml();
                if (color.HasValue && color.Value.A > 0)
                {
                    path.Stroke = new XamlMedia.SolidColorBrush { Color = color.Value };
                    path.StrokeThickness = ((VectorStyle)style).Line.Width;
                }
            }

            return path;
        }

        public static Rect RoundToPixel(Rect dest)
        {
            // To get seamless aligning you need to round the 
            // corner coordinates to pixel. The new width and
            // height will be a result of that.
            dest = new Rect(
                Math.Round(dest.Left),
                Math.Round(dest.Top),
                Math.Round(dest.Right) - Math.Round(dest.Left),
                Math.Round(dest.Bottom) - Math.Round(dest.Top));
            return dest;
        }

        public static void PositionRaster(UIElement renderedGeometry, BoundingBox boundingBox, IReadOnlyViewport viewport)
        {
            UpdateRenderTransform(renderedGeometry, viewport);

            // since the render transform will take care of the rotation, calculate top-left using unrotated viewport
            var topLeft = viewport.WorldToScreenUnrotated(boundingBox.TopLeft);
            var rectWidthPixels = boundingBox.Width / viewport.Resolution;
            var rectHeightPixels = boundingBox.Height / viewport.Resolution;
            ((XamlMedia.RectangleGeometry)((XamlShapes.Path)renderedGeometry).Data).Rect =
                RoundToPixel(new Rect(topLeft.X, topLeft.Y, rectWidthPixels, rectHeightPixels));
        }

        private static void UpdateRenderTransform(UIElement renderedGeometry, IReadOnlyViewport viewport)
        {
            var matrix = XamlMedia.Matrix.Identity;

            if (viewport.IsRotated)
            {
                var center = viewport.WorldToScreen(viewport.Center);
                MatrixHelper.RotateAt(ref matrix, viewport.Rotation, center.X, center.Y);
            }

            renderedGeometry.RenderTransform = new XamlMedia.MatrixTransform { Matrix = matrix };
        }

        public static void PositionGeometry(XamlShapes.Shape renderedGeometry, IReadOnlyViewport viewport)
        {
            CounterScaleLineWidth(renderedGeometry, viewport.Resolution);
            var matrixTransform = new XamlMedia.MatrixTransform { Matrix = CreateTransformMatrix(viewport) };
            renderedGeometry.RenderTransform = matrixTransform;

            if (renderedGeometry.Fill != null)
                renderedGeometry.Fill.Transform = matrixTransform.Inverse as XamlMedia.MatrixTransform;
        }

        public static void CounterScaleLineWidth(UIElement renderedGeometry, double resolution)
        {
            // #linewidthhack
            // When the RenderTransform Matrix is applied the width of the line
            // is scaled along with the rest. We want the outline to have a fixed
            // width independent of the scale. So here we counter scale using
            // the orginal width stored in the Tag.
            var path = renderedGeometry as XamlShapes.Path;
            if (path?.Tag is double) path.StrokeThickness = (path.Tag as double?).Value * resolution;
        }
    }
}
