using Mapsui.Extensions;

namespace Mapsui.Widgets.MouseCoordinatesWidget;

public class MouseCoordinatesWidget : TextBox, IWidgetExtended
{
    public Map Map { get; }
    
    public MouseCoordinatesWidget(Map map)
    {
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Bottom;
        Text = "Mouse Position";
        Map = map;
    }

    public bool HandleWidgetMoving(Navigator navigator, MPoint position, WidgetArgs args)
    {
        var worldPosition = Map.Navigator.Viewport.ScreenToWorld(position);
        // update the Mouse position
        Text = $"{worldPosition.X:F0}, {worldPosition.Y:F0}";
        return false;
    }

    public bool HandleWidgetTouching(Navigator navigator, MPoint position, WidgetArgs args)
    {
        return false;
    }

    public bool HandleWidgetTouched(Navigator navigator, MPoint position, WidgetArgs args)
    {
        return false;
    }
}
