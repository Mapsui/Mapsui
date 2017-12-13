using System.Collections.Generic;
using Mapsui.Geometries;

namespace Mapsui.Widgets
{
    static class WidgetTouch
    {
        /// <returns>
        /// Returns the first Widget in the list that contains the screenPosition
        /// within it's Envelope. Returns null if there are none.
        /// </returns>
        public static IWidget GetWidget(Point screenPosition, Point startScreenPosition, IEnumerable<IWidget> widgets)
        {
            foreach (var widget in widgets)
            {
                // Also check for start position because it shoudl be click on the widget, not a drag that ends above the widget.
                if (widget.Envelope.Contains(screenPosition) && widget.Envelope.Contains(startScreenPosition))
                    return widget;
            }
            return null;
        }
    }
}
