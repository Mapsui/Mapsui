using Mapsui.Utilities;
using System;

namespace Mapsui.Extensions;

public static class ViewportStateExtensions
{
    /// <summary>
    /// IsRotated is true, when viewport displays map rotated
    /// </summary>
    public static MSection GetSection(this IViewportState viewport)
    {
        if (viewport.Extent is null) throw new ArgumentNullException(nameof(viewport.Extent));
        return new MSection(viewport.Extent!, viewport.Resolution);
    }

    /// <summary>
    /// IsRotated is true, when viewport displays map rotated
    /// </summary>
    public static bool IsRotated(this IViewportState viewport) =>
        !double.IsNaN(viewport.Rotation) && viewport.Rotation > Constants.Epsilon
        && viewport.Rotation < 360 - Constants.Epsilon;

    /// <summary>
    /// Calculates extent from the viewport.
    /// </summary>
    /// <remarks>
    /// This MRect is horizontally and vertically aligned, even if the viewport
    /// is rotated. So this MRect perhaps contain parts, that are not visible.
    /// </remarks>
    public static MRect GetExtent(this IViewportState viewportState)
    {
        // todo: Find out how this method related to Viewport.UpdateExtent 

        // calculate the window extent 
        var halfSpanX = viewportState.Width * viewportState.Resolution * 0.5;
        var halfSpanY = viewportState.Height * viewportState.Resolution * 0.5;
        var minX = viewportState.CenterX - halfSpanX;
        var minY = viewportState.CenterY - halfSpanY;
        var maxX = viewportState.CenterX + halfSpanX;
        var maxY = viewportState.CenterY + halfSpanY;

        if (!viewportState.IsRotated())
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
