using Mapsui.Manipulations;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Widgets;

public static class WidgetInput
{
    /// <summary>
    /// Gets the widgets selected by a touched positions
    /// </summary>
    /// <param name="position">The screen position in device independent units (or DIP or DP)</param>
    /// <param name="map">The map to get the widgets from.</param>
    /// <returns>
    /// Returns the Widgets in the list that contain the screenPosition
    /// within it's Envelope. Returns null if there are none.
    /// </returns>
    public static IEnumerable<IWidget> GetWidgetsAtPosition(ScreenPosition position, Map map)
    {
        var touchedWidgets = new List<IWidget>();

        var touchableWidgets = map.GetWidgetsOfMapAndLayers()
            .Where(w => w.Enabled && w.InputTransparent == false)
            .Reverse().ToArray();

        foreach (var widget in touchableWidgets)
        {
            // There are two options:
            if (widget.InputAreaType == InputAreaType.Map) // 1) When type is 'Map' the widget is always touched.
                touchedWidgets.Add(widget);
            else if (widget.Envelope?.Contains(position.X, position.Y) ?? false) // 2) When type is 'Widget' the position needs to be within the envelope.
                touchedWidgets.Add(widget);
        }

        return touchedWidgets;
    }
}
