using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Widgets;

public static class WidgetTouch
{
    /// <summary>
    /// Gets the widgets selected by a touched positions
    /// </summary>
    /// <param name="screenPosition">The screen position in device independent units (or DIP or DP)</param>
    /// <param name="startScreenPosition">The start screen position in device independent units (or DIP or DP)</param>
    /// <param name="widgets">The widgets to select from.</param>
    /// <returns>
    /// Returns the Widgets in the list that contain the screenPosition
    /// within it's Envelope. Returns null if there are none.
    /// </returns>
    public static IEnumerable<ITouchableWidget> GetTouchedWidgets(MPoint screenPosition, MPoint startScreenPosition,
        IEnumerable<ITouchableWidget> widgets)
    {
        var touchedWidgets = new List<ITouchableWidget>();

        foreach (var widget in widgets.Where(w => w is ITouchableWidget).Where(w => w.Enabled).Reverse())
        {
            // There are two possible TouchableAreaTypes

            if (widget.TouchableArea == TouchableAreaType.Viewport)
            {
                // Widget is always touched
                touchedWidgets.Add(widget);
                continue;
            }

            // Also check for start position, because it should be click on the widget,
            // not a drag that ends above the widget.
            if ((widget.Envelope?.Contains(screenPosition) ?? false) &&
                (widget.Envelope?.Contains(startScreenPosition) ?? false))
                touchedWidgets.Add(widget);
        }

        return touchedWidgets;
    }
}
