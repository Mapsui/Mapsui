using Mapsui.Widgets.BoxWidgets;
using System;

namespace Mapsui.Widgets.ButtonWidgets;

public class TextButtonWidget : TextBoxWidget, ITouchableWidget
{
    /// <summary>
    /// Event which is called if widget is touched
    /// </summary>
    public event EventHandler<WidgetTouchedEventArgs>? Touched;

    /// <summary>
    /// Type of area to use for touch events
    /// </summary>
    public TouchableAreaType TouchableArea => TouchableAreaType.Widget;

    /// <summary>
    /// Handle touch to Widget
    /// </summary>
    /// <param name="navigator">Navigator used by map</param>
    /// <param name="position">Position of touch</param>
    /// <param name="args">Arguments for widget event</param>
    /// <returns>True, if touch is handled</returns>
    public bool HandleWidgetTouched(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        Touched?.Invoke(this, args);

        return args.Handled;
    }

    public bool HandleWidgetTouching(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        return false;
    }

    public bool HandleWidgetMoving(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        return false;
    }
}
