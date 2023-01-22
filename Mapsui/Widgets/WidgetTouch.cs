using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Widgets;

public static class WidgetTouch
{
    /// <summary>
    /// Gets the widget selected by touch positions
    /// </summary>
    /// <param name="screenPosition">The screen position in device independent units (or DIP or DP)</param>
    /// <param name="startScreenPosition">The start screen position in device independent units (or DIP or DP)</param>
    /// <param name="widgets">The widgets to select from.</param>
    /// <returns>
    /// Returns the first Widget in the list that contains the screenPosition
    /// within it's Envelope. Returns null if there are none.
    /// </returns>
    public static IEnumerable<IWidget> GetTouchedWidget(MPoint screenPosition, MPoint startScreenPosition,
        IEnumerable<IWidget> widgets)
    {
        var touchedWidgets = new List<IWidget>();

        foreach (var widget in widgets.Reverse())
        {
            // Also check for start position because it should be click on the widget,
            // not a drag that ends above the widget.
            if (widget.Envelope != null &&
                widget.Enabled &&
                widget.Envelope.Contains(screenPosition) &&
                widget.Envelope.Contains(startScreenPosition))
                touchedWidgets.Add(widget);
        }

        return touchedWidgets;
    }
}
