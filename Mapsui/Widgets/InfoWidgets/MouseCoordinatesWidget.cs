using Mapsui.Extensions;
using Mapsui.Manipulations;
using Mapsui.Widgets.BoxWidgets;

namespace Mapsui.Widgets.InfoWidgets;

/// <summary>
/// Widget that shows actual mouse coordinates in a TextBox
/// </summary>
public class MouseCoordinatesWidget : TextBoxWidget, ITouchableWidget
{
    public TouchableAreaType TouchableArea => TouchableAreaType.Viewport;

    public MouseCoordinatesWidget()
    {
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Bottom;
        Text = "Mouse Position";
    }

    public bool OnTapped(Navigator navigator, ScreenPosition position, WidgetEventArgs e)
    {
        return false;
    }

    public bool OnPointerPressed(Navigator navigator, ScreenPosition position, WidgetEventArgs e)
    {
        return false;
    }

    public bool OnPointerMoved(Navigator navigator, ScreenPosition position, WidgetEventArgs e)
    {
        var worldPosition = navigator.Viewport.ScreenToWorld(position);
        // update the Mouse position
        Text = $"{worldPosition.X:F0}, {worldPosition.Y:F0}";
        return false;
    }

    public bool OnPointerReleased(Navigator navigator, ScreenPosition position, WidgetEventArgs e)
    {
        return false;
    }
}
