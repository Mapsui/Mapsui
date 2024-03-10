using Mapsui.Manipulations;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Widgets;

public static class WidgetTouch
{
    /// <summary>
    /// Gets the widgets selected by a touched positions
    /// </summary>
    /// <param name="screenPosition">The screen position in device independent units (or DIP or DP)</param>
    /// <param name="map">The map to get the widgets from.</param>
    /// <returns>
    /// Returns the Widgets in the list that contain the screenPosition
    /// within it's Envelope. Returns null if there are none.
    /// </returns>
    public static IEnumerable<IWidget> GetTouchedWidgets(ScreenPosition screenPosition, Map map)
    {
        var touchedWidgets = new List<IWidget>();

        var touchableWidgets = map.GetWidgetsOfMapAndLayers().Where(w => w.Enabled).Reverse().ToList();

        foreach (var widget in touchableWidgets)
        {
            // There are two options:
            if (widget.WidgetAreaType == WidgetAreaType.Map) // 1) With 'Map' type the widget is always touched.
                touchedWidgets.Add(widget);
            else if (widget.Envelope?.Contains(screenPosition.X, screenPosition.Y) ?? false) // 2) With 'Widget' type the position needs to be within the envelope
                touchedWidgets.Add(widget);
        }

        return touchedWidgets;
    }
}
