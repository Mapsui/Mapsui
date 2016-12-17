#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using XamlBrush = System.Windows.Media.Brush;
using XamlColor = System.Windows.Media.Color;
using System.Windows;
using System.Collections.Generic;
#else
using XamlBrush = Windows.UI.Xaml.Media.Brush;
using Windows.UI.Xaml.Media;
#endif
using Mapsui.Styles;

namespace Mapsui.Rendering.Xaml
{
    public static class StyleConverter
    {
        public static DoubleCollection MapsuiPentoXaml(PenStyle penStyle)
        {
            switch (penStyle)
            {
                case PenStyle.Dash:
                    return new DoubleCollection {2, 2};
                case PenStyle.DashDot:
                    return new DoubleCollection {2, 2, 1, 2};
                case PenStyle.DashDotDot:
                    return new DoubleCollection {2, 2, 1, 2, 1, 2};
                case PenStyle.Dot:
                    return new DoubleCollection {1, 2};
                case PenStyle.LongDash:
                    return new DoubleCollection {2, 3};
                case PenStyle.LongDashDot:
                    return new DoubleCollection {2, 3, 1, 3};
                case PenStyle.ShortDash:
                    return new DoubleCollection {2, 1};
                case PenStyle.ShortDashDot:
                    return new DoubleCollection {2, 1, 1, 1};
                case PenStyle.ShortDashDotDot:
                    return new DoubleCollection {2, 1, 1, 1, 1, 1};
                case PenStyle.ShortDot:
                    return new DoubleCollection {1, 1};
            }

            return null;
        }

        public static XamlBrush MapsuiBrushToXaml(Styles.Brush brush, BrushCache brushCache = null)
        {
#if !NETFX_CORE
            if (brush == null) return null;
            switch (brush.FillStyle)
            {
                case FillStyle.Cross:
                    return CreateHatchBrush(brush, 12, 10, new List<Geometry> { Geometry.Parse("M 0 0 l 10 10"), Geometry.Parse("M 0 10 l 10 -10") });
                case FillStyle.BackwardDiagonal:
                    return CreateHatchBrush(brush, 10, 10, new List<Geometry> { Geometry.Parse("M 0 10 l 10 -10"), Geometry.Parse("M -0.5 0.5 l 10 -10"), Geometry.Parse("M 8 12 l 10 -10") });                    
                case FillStyle.Bitmap:
                    return CreateImageBrush(brush, brushCache);
                case FillStyle.Dotted:
                    return DottedBrush(brush);
                case FillStyle.DiagonalCross:
                    return CreateHatchBrush(brush, 10, 10, new List<Geometry> { Geometry.Parse("M 0 0 l 10 10"), Geometry.Parse("M 0 10 l 10 -10") });
                case FillStyle.ForwardDiagonal:
                    return CreateHatchBrush(brush, 10, 10, new List<Geometry> { Geometry.Parse("M -1 9 l 10 10"), Geometry.Parse("M 0 0 l 10 10"), Geometry.Parse("M 9 -1 l 10 10") });
                case FillStyle.Hollow:
                    return new SolidColorBrush(Colors.Transparent);
                case FillStyle.Horizontal:
                    return CreateHatchBrush(brush, 10, 10, new List<Geometry> { Geometry.Parse("M 0 5 h 10") });
                case FillStyle.Solid:
                    return new SolidColorBrush(brush.Color != null ? brush.Color.ToXaml() : brush.Background != null ? brush.Color.ToXaml() : Colors.Transparent);
                case FillStyle.Vertical:
                    return CreateHatchBrush(brush, 10, 10, new List<Geometry> { Geometry.Parse("M 5 0 l 0 10") });
                default:
                    return (brush.Color != null) ? new SolidColorBrush(brush.Color.ToXaml()) : null;
            }
#else
            return new SolidColorBrush(brush.Color.ToXaml());
#endif
        }

#if !SILVERLIGHT && !NETFX_CORE
        private static XamlColor GetColor(Styles.Color color)
        {
            return color == null ? Colors.Black : color.ToXaml();
        }

        private static VisualBrush CreateHatchBrush(Styles.Brush brush, int viewbox, int viewport, IEnumerable<Geometry> geometries)
        {
            var elements = new List<UIElement>();           
            if (brush.Background != null)
                elements.Add(CreateBackground(brush.Background, viewbox));

            var stroke = new SolidColorBrush(GetColor(brush.Color));          
            var canvas = new Canvas();
            foreach (var geometry in geometries)
            {
                canvas.Children.Add(new Path
                {
                    Stroke = stroke,
                    Data = geometry,
                    StrokeThickness = 1,
                    SnapsToDevicePixels = false
                });
            }
            canvas.Arrange(new Rect(0, 0, viewbox, viewbox));
            elements.Add(canvas);
            return CreatePatternVisual(elements, viewport, viewbox);
        }

        private static ImageBrush CreateImageBrush(Styles.Brush brush, BrushCache brushCache = null)
        {
            return brushCache != null ? brushCache.GetImageBrush(brush.BitmapId, CreateImageBrush) : CreateImageBrush(BitmapRegistry.Instance.Get(brush.BitmapId));
        }

        private static ImageBrush CreateImageBrush(System.IO.Stream stream)
        {
            var bmp = stream.CreateBitmapImage();

            var imageBrush = new ImageBrush(bmp)
            {
                Viewbox = new Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight),
                Viewport = new Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight),
                ViewportUnits = BrushMappingMode.Absolute,
                ViewboxUnits = BrushMappingMode.Absolute,
                TileMode = TileMode.Tile
            };

            return imageBrush;
        }

        private static VisualBrush DottedBrush(Styles.Brush brush)
        {
            const int viewboxSize = 12;
            const int viewportSize = 10;

            var elements = new List<UIElement>();
            if(brush.Background != null)
                elements.Add(CreateBackground(brush.Background, viewboxSize));

            elements.Add(new Ellipse
            {
                Fill = new SolidColorBrush(GetColor(brush.Color)),
                Width = 10,
                Height = 10
            });

            return CreatePatternVisual(elements, viewportSize, viewboxSize);
        }

        private static Rectangle CreateBackground(Styles.Color color, int size)
        {
            return new Rectangle
            {
                Fill = color == null ? new SolidColorBrush(Colors.Transparent) : new SolidColorBrush(GetColor(color)),
                Width = size,
                Height = size
            };
        }

        private static VisualBrush CreatePatternVisual(IEnumerable<UIElement> elements, int viewPort, int viewbox)
        {
            var canvas = new Canvas();
            foreach (var vis in elements)
            {
                canvas.Children.Add(vis);
            }

            var visualBrush = new VisualBrush
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, viewPort, viewPort),
                ViewportUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(0, 0, viewbox, viewbox),
                ViewboxUnits = BrushMappingMode.Absolute,
                Visual = canvas,                
            };
            canvas.Arrange(new Rect(0, 0, viewbox, viewbox));
            return visualBrush;
        }
#endif
    }
}
