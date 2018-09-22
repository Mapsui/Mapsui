using Mapsui.Widgets.CenterCross;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Mapsui.Rendering.Xaml
{
    public static class CenterCrossWidgetRenderer
    {
        private const float StrokeExternal = 4;
        private const float StrokeInternal = 2;

        private static Brush _brushCenterCrossInternal;
        private static Brush _brushCenterCrossExternal;

        public static void Draw(Canvas canvas, CenterCrossWidget centerCross)
        {

            _brushCenterCrossInternal = new SolidColorBrush(centerCross.Color.ToXaml());
            _brushCenterCrossExternal = new SolidColorBrush(centerCross.Halo.ToXaml());

            var centerX = (float)centerCross.Map.Viewport.Width * 0.5f;
            var centerY = (float)centerCross.Map.Viewport.Height * 0.5f;
            var halfWidth = centerCross.Width * 0.5f + (StrokeExternal - StrokeInternal) * centerCross.Scale;
            var halfHeight = centerCross.Height * 0.5f;
            var haloSize = (StrokeExternal - StrokeInternal) * centerCross.Scale;

            var line = new Line();
            line.X1 = centerX - halfWidth - haloSize;
            line.Y1 = centerY;
            line.X2 = centerX + halfWidth + haloSize;
            line.Y2 = centerY;
            line.Stroke = _brushCenterCrossExternal;
            line.StrokeThickness = StrokeExternal;
            line.StrokeStartLineCap = PenLineCap.Square;
            line.StrokeEndLineCap = PenLineCap.Square;
            canvas.Children.Add(line);

            line = new Line();
            line.X1 = centerX;
            line.Y1 = centerY - halfHeight - haloSize;
            line.X2 = centerX;
            line.Y2 = centerY + halfHeight + haloSize;
            line.Stroke = _brushCenterCrossExternal;
            line.StrokeThickness = StrokeExternal;
            line.StrokeStartLineCap = PenLineCap.Square;
            line.StrokeEndLineCap = PenLineCap.Square;
            canvas.Children.Add(line);

            line = new Line();
            line.X1 = centerX - halfWidth;
            line.Y1 = centerY;
            line.X2 = centerX + halfWidth;
            line.Y2 = centerY;
            line.Stroke = _brushCenterCrossInternal;
            line.StrokeThickness = StrokeInternal;
            line.StrokeStartLineCap = PenLineCap.Square;
            line.StrokeEndLineCap = PenLineCap.Square;
            canvas.Children.Add(line);

            line = new Line();
            line.X1 = centerX;
            line.Y1 = centerY - halfHeight;
            line.X2 = centerX;
            line.Y2 = centerY + halfHeight;
            line.Stroke = _brushCenterCrossInternal;
            line.StrokeThickness = StrokeInternal;
            line.StrokeStartLineCap = PenLineCap.Square;
            line.StrokeEndLineCap = PenLineCap.Square;
            canvas.Children.Add(line);
        }
    }
}