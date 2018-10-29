using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Mapsui.Geometries;
using Mapsui.Widgets;
using Mapsui.Widgets.Zoom;

namespace Mapsui.Rendering.Xaml.XamlWidgets
{
    public class ZoomInOutWidgetRenderer : IXamlWidgetRenderer

    {
    private const float Stroke = 3;

    private static Brush _brushStroke;
    private static Brush _brushBackground;
    private static Brush _brushText;

    public void Draw(Canvas canvas, IReadOnlyViewport viewport, IWidget widget)
    {
        var zoomInOut = (ZoomInOutWidget) widget;

        _brushStroke = new SolidColorBrush(zoomInOut.StrokeColor.ToXaml());
        _brushStroke.Opacity = zoomInOut.Opacity;
        _brushBackground = new SolidColorBrush(zoomInOut.BackColor.ToXaml());
        _brushBackground.Opacity = zoomInOut.Opacity;
        _brushText = new SolidColorBrush(zoomInOut.TextColor.ToXaml());
        _brushText.Opacity = zoomInOut.Opacity;

        var posX = zoomInOut.CalculatePositionX(0, (float) canvas.ActualWidth,
            zoomInOut.Orientation == Widgets.Zoom.Orientation.Vertical ? zoomInOut.Size : zoomInOut.Size * 2 - Stroke);
        var posY = zoomInOut.CalculatePositionY(0, (float) canvas.ActualHeight,
            zoomInOut.Orientation == Widgets.Zoom.Orientation.Vertical ? zoomInOut.Size * 2 - Stroke : zoomInOut.Size);

        // Draw a rect for zoom in button
        var rect = new Rectangle();
        rect.Width = zoomInOut.Size;
        rect.Height = zoomInOut.Size;
        rect.Stroke = _brushStroke;
        rect.StrokeThickness = Stroke;
        rect.Fill = _brushBackground;
        rect.RadiusX = 2;
        rect.RadiusY = 2;
        Canvas.SetLeft(rect, posX);
        Canvas.SetTop(rect, posY);
        canvas.Children.Add(rect);

        // Draw a rect for zoom in button
        rect = new Rectangle();
        rect.Width = zoomInOut.Size;
        rect.Height = zoomInOut.Size;
        rect.Stroke = _brushStroke;
        rect.StrokeThickness = Stroke;
        rect.Fill = _brushBackground;
        rect.RadiusX = 2;
        rect.RadiusY = 2;
        Canvas.SetLeft(rect,
            zoomInOut.Orientation == Widgets.Zoom.Orientation.Vertical ? posX : posX + rect.Width - Stroke);
        Canvas.SetTop(rect,
            zoomInOut.Orientation == Widgets.Zoom.Orientation.Vertical ? posY + rect.Height - Stroke : posY);
        canvas.Children.Add(rect);

        // Draw +
        var line = new Line();
        line.X1 = posX + zoomInOut.Size * 0.3;
        line.Y1 = posY + zoomInOut.Size * 0.5;
        line.X2 = posX + zoomInOut.Size * 0.7;
        line.Y2 = posY + zoomInOut.Size * 0.5;
        line.Stroke = _brushText;
        line.StrokeThickness = Stroke;
        line.StrokeStartLineCap = PenLineCap.Square;
        line.StrokeEndLineCap = PenLineCap.Square;
        canvas.Children.Add(line);

        line = new Line();
        line.X1 = posX + zoomInOut.Size * 0.5;
        line.Y1 = posY + zoomInOut.Size * 0.3;
        line.X2 = posX + zoomInOut.Size * 0.5;
        line.Y2 = posY + zoomInOut.Size * 0.7;
        line.Stroke = _brushText;
        line.StrokeThickness = Stroke;
        line.StrokeStartLineCap = PenLineCap.Square;
        line.StrokeEndLineCap = PenLineCap.Square;
        canvas.Children.Add(line);

        // Draw -
        line = new Line();
        if (zoomInOut.Orientation == Widgets.Zoom.Orientation.Vertical)
        {
            line.X1 = posX + zoomInOut.Size * 0.3;
            line.Y1 = posY - Stroke + zoomInOut.Size * 1.5;
            line.X2 = posX + zoomInOut.Size * 0.7;
            line.Y2 = posY - Stroke + zoomInOut.Size * 1.5;
        }
        else
        {
            line.X1 = posX - Stroke + zoomInOut.Size * 1.3;
            line.Y1 = posY + zoomInOut.Size * 0.5;
            line.X2 = posX - Stroke + zoomInOut.Size * 1.7;
            line.Y2 = posY + zoomInOut.Size * 0.5;
        }

        line.Stroke = _brushText;
        line.StrokeThickness = Stroke;
        line.StrokeStartLineCap = PenLineCap.Square;
        line.StrokeEndLineCap = PenLineCap.Square;
        canvas.Children.Add(line);

        if (zoomInOut.Orientation == Widgets.Zoom.Orientation.Vertical)
            zoomInOut.Envelope = new BoundingBox(posX, posY, posX + rect.Width, posY + rect.Width * 2 - Stroke);
        else
            zoomInOut.Envelope = new BoundingBox(posX, posY, posX + rect.Width * 2 - Stroke, posY + rect.Width);
    }
    }
}