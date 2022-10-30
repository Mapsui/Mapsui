using System;
using System.Collections.Generic;
using System.Text;

namespace Mapsui.Extensions
{
    public static class ViewportStateExtensions
    {
        /// <summary>
        /// Calculates extent from the viewport
        /// </summary>
        public static MRect GetExtent(this ViewportState viewportState)
        {
            // calculate the window extent 
            var halfSpanX = viewportState.Width * viewportState.Resolution * 0.5;
            var halfSpanY = viewportState.Height * viewportState.Resolution * 0.5;
            var minX = viewportState.CenterX - halfSpanX;
            var minY = viewportState.CenterY - halfSpanY;
            var maxX = viewportState.CenterX + halfSpanX;
            var maxY = viewportState.CenterY + halfSpanY;

            if (!viewportState.IsRotated)
            {
                return new MRect(minX, minY, maxX, maxY);
            }
            else
            {
                var windowExtent = new MQuad
                {
                    BottomLeft = new MPoint(minX, minY),
                    TopLeft = new MPoint(minX, maxY),
                    TopRight = new MPoint(maxX, maxY),
                    BottomRight = new MPoint(maxX, minY)
                };

                // Calculate the extent that will encompass a rotated viewport (slightly larger - used for tiles).
                // Perform rotations on corner offsets and then add them to the Center point.
                return windowExtent.Rotate(-viewportState.Rotation, viewportState.CenterX, viewportState.CenterY).ToBoundingBox();
            }
        }
    }
}
