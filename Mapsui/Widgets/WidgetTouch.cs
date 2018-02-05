using System.Collections.Generic;
using System.Linq;
using Mapsui.Geometries;

namespace Mapsui.Widgets
{
    static class WidgetTouch
    {
        /// <returns>
        /// Returns the first Widget in the list that contains the screenPosition
        /// within it's Envelope. Returns null if there are none.
        /// </returns>
        public static IWidget GetWidget(Point screenPosition, Point startScreenPosition, double scale,
            IEnumerable<IWidget> widgets)
        {
            var scaledScreenPosition = new Point(screenPosition.X / scale, screenPosition.Y / scale);
            var scaledStartScreenPosition = new Point(
                startScreenPosition.X / scale, 
                startScreenPosition.Y / scale);

            foreach (var widget in widgets.Reverse())
            {
                // Also check for start position because it shoudl be click on the widget, not a drag that ends above the widget.
                if (widget.Envelope != null &&
                    widget.Envelope.Contains(scaledScreenPosition) && 
                    widget.Envelope.Contains(scaledStartScreenPosition))
                    return widget;
            }
            return null;
        }
    }
}
