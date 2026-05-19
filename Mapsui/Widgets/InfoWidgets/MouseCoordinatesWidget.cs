using Mapsui.Extensions;
using Mapsui.Widgets.BoxWidgets;

namespace Mapsui.Widgets.InfoWidgets;

/// <summary>
/// Widget that shows actual mouse coordinates in a TextBox
/// </summary>
public class MouseCoordinatesWidget : TextBoxWidget
{
    public MouseCoordinatesWidget()
    {
        InputAreaType = InputAreaType.Map;
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Bottom;
        Text = "Mouse Position";
    }

    public override void OnPointerMoved(WidgetEventArgs e)
    {
        var worldPosition = e.Map.Navigator.Viewport.ScreenToWorld(e.ScreenPosition);
        Text = $"{worldPosition.X:F0}, {worldPosition.Y:F0}";
        // Trigger a redraw so the updated coordinates are visible immediately on mouse move.
        // Without this the display only refreshes when something else (e.g. a pan or zoom) causes a repaint.
        e.Map.RefreshGraphics();
    }
}
