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

    public override bool OnPointerMoved(Navigator navigator, WidgetEventArgs e)
    {
        var worldPosition = navigator.Viewport.ScreenToWorld(e.ScreenPosition);
        // update the Mouse position
        Text = $"{worldPosition.X:F0}, {worldPosition.Y:F0}";
        return false;
    }
}
