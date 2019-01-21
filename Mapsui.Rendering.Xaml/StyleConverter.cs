using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using XamlBrush = System.Windows.Media.Brush;
using XamlColor = System.Windows.Media.Color;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Mapsui.Styles;

namespace Mapsui.Rendering.Xaml
{
    public static class StyleConverter
    {
        public static DoubleCollection MapsuiPentoXaml(PenStyle penStyle, float[] dashArray = null)
        {
            switch (penStyle)
            {
                case PenStyle.UserDefined:
                    if (dashArray == null || dashArray.Length == 0 || dashArray.Length % 2 != 0)
                        return new DoubleCollection { 1, 0 };
                    var dash = new DoubleCollection(dashArray.Length);
                    for (var i = 0; i < dashArray.Length; i++)
                    {
                        dash.Add(dashArray[i]);
                    }
                    return dash;
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

        public static PenLineCap MapsuiStrokeCaptoPenLineCap(PenStrokeCap penStrokeCap)
        {
            switch(penStrokeCap)
            {
                case PenStrokeCap.Butt:
                    return PenLineCap.Flat;
                case PenStrokeCap.Round:
                    return PenLineCap.Round;
                case PenStrokeCap.Square:
                    return PenLineCap.Square;
                default:
                    return PenLineCap.Flat;
            }
        }

        public static PenLineJoin MapsuiStrokeJointoPenLineJoin(StrokeJoin penStrokeJoin)
        {
            switch (penStrokeJoin)
            {
                case StrokeJoin.Miter:
                    return PenLineJoin.Miter;
                case StrokeJoin.Round:
                    return PenLineJoin.Round;
                case StrokeJoin.Bevel:
                    return PenLineJoin.Bevel;
                default:
                    return PenLineJoin.Miter;
            }
        }

        public static XamlBrush MapsuiBrushToXaml(Styles.Brush brush, SymbolCache symbolCache = null, double rotate = 0)
        {
            if (brush == null) return null;
            switch (brush.FillStyle)
            {
                case FillStyle.Cross:
                    return CreateHatchBrush(brush, 12, 10, new List<Geometry> { Geometry.Parse("M 0 0 l 10 10"), Geometry.Parse("M 0 10 l 10 -10") });
                case FillStyle.BackwardDiagonal:
                    return CreateHatchBrush(brush, 10, 10, new List<Geometry> { Geometry.Parse("M 0 10 l 10 -10"), Geometry.Parse("M -0.5 0.5 l 10 -10"), Geometry.Parse("M 8 12 l 10 -10") });                    
                case FillStyle.Bitmap:
                    return GetOrCreateBitmapImage(brush, symbolCache).ToTiledImageBrush();
                case FillStyle.BitmapRotated:
                    RotateTransform aRotateTransform = new RotateTransform();
                    aRotateTransform.CenterX = 0.5;
                    aRotateTransform.CenterY = 0.5;
                    aRotateTransform.Angle = rotate;
                    var b = GetOrCreateBitmapImage(brush, symbolCache).ToTiledImageBrush();
                    b.RelativeTransform = aRotateTransform;
                    return b;
                case FillStyle.Svg:
                    return null;
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
        }

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

        private static BitmapImage GetOrCreateBitmapImage(Styles.Brush brush, SymbolCache symbolCache = null)
        {
            return symbolCache != null ? 
                (BitmapImage)symbolCache.GetOrCreate(brush.BitmapId): 
                ((System.IO.Stream)BitmapRegistry.Instance.Get(brush.BitmapId)).ToBitmapImage();
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
    }
}