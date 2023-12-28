using Mapsui.Extensions;
using Mapsui.Widgets.BoxWidgets;

namespace Mapsui.Widgets.InfoWidgets;

/// <summary>
/// Widget that shows actual mouse coordinates in a TextBox
/// </summary>
public class MouseCoordinatesWidget : TextBoxWidget, ITouchableWidget
{
    private readonly Map _map;

    public TouchableAreaType TouchableArea => TouchableAreaType.Viewport;

    public MouseCoordinatesWidget(Map map)
    {
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Bottom;
        Text = "Mouse Position";
        _map = map;
    }

    public bool HandleWidgetMoving(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        // Upadte mouse position
        var worldPosition = navigator.Viewport.ScreenToWorld(position);
        var newText = $"{worldPosition.X:F0}, {worldPosition.Y:F0}";

        if (Text != newText)
        {
            Text = newText;
            _map.RefreshGraphics();
        }

        return false;
    }

    public bool HandleWidgetTouching(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        return false;
    }

    public bool HandleWidgetTouched(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        return false;
    }
}
