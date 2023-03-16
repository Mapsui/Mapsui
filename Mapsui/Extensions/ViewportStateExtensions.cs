using Mapsui.Utilities;

namespace Mapsui.Extensions;

public static class ViewportStateExtensions
{
    /// <summary>
    /// True if Width and Height are not zero
    /// </summary>
    public static bool HasSize(this ViewportState viewport) =>
        viewport.Width > 0 && viewport.Height > 0;

    /// <summary> World To Screen Translation of a Rect </summary>
    /// <param name="viewport">view Port</param>
    /// <param name="rect">rect</param>
    /// <returns>Transformed rect</returns>
    public static MRect WorldToScreen(this ViewportState viewport, MRect rect)
    {
        var min = viewport.WorldToScreen(rect.Min);
        var max = viewport.WorldToScreen(rect.Max);
        return new MRect(min.X, min.Y, max.X, max.Y);
    }

    /// <summary>
    /// IsRotated is true, when viewport displays map rotated
    /// </summary>
    public static MSection ToSection(this ViewportState viewport)
    {
        return new MSection(viewport.ToExtent(), viewport.Resolution);
    }

    /// <summary>
    /// IsRotated is true, when viewport displays map rotated
    /// </summary>
    public static bool IsRotated(this ViewportState viewport) =>
        !double.IsNaN(viewport.Rotation) && viewport.Rotation > Constants.Epsilon
        && viewport.Rotation < 360 - Constants.Epsilon;

    /// <summary>
    /// Calculates extent from the viewport.
    /// </summary>
    /// <remarks>
    /// This MRect is horizontally and vertically aligned, even if the viewport
    /// is rotated. So this MRect perhaps contain parts, that are not visible.
    /// </remarks>
    public static MRect ToExtent(this ViewportState viewportState)
    {
        // todo: Find out how this method relates to Viewport.UpdateExtent 

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

    /// <summary>
    /// Converts X/Y in world units to a point in device independent unit (or DIP or DP),
    /// respecting rotation
    /// </summary>
    /// <param name="worldPosition">Coordinate in world units</param>
    /// <returns>MPoint in screen pixels</returns>  
    public static MPoint WorldToScreen(this ViewportState viewportState, MPoint worldPosition)
    {
        return viewportState.WorldToScreen(worldPosition.X, worldPosition.Y);
    }

    /// <summary>
    /// Converts a point in screen pixels to one in screen units, respecting rotation
    /// </summary>
    /// <param name="screenPosition">Coordinate in screen units</param>
    /// <returns>MPoint in world units</returns>
    /// <inheritdoc />
    public static MPoint ScreenToWorld(this ViewportState viewportState, MPoint screenPosition)
    {
        return viewportState.ScreenToWorld(screenPosition.X, screenPosition.Y);
    }

    /// <summary>
    /// Converts X/Y in screen pixels to a point in screen units, respecting rotation
    /// </summary>
    /// <param name="x">Screen position x coordinate</param>
    /// <param name="y">Screen position y coordinate</param>
    /// <returns>MPoint in world units</returns>
    public static MPoint ScreenToWorld(this ViewportState viewportState, double screenX, double screenY)
    {
        var (x, y) = viewportState.ScreenToWorldXY(screenX, screenY);
        return new MPoint(x, y);
    }

    /// <summary>
    /// Converts X/Y in world units to a point in device independent units (or DIP or DP),
    /// respecting rotation
    /// </summary>
    /// <param name="worldX">X coordinate in world units</param>
    /// <param name="worldY">Y coordinate in world units</param>
    /// <returns>MPoint in screen pixels</returns>
    public static MPoint WorldToScreen(this ViewportState viewportState, double worldX, double worldY)
    {
        var (x, y) = viewportState.WorldToScreenXY(worldX, worldY);
        return new MPoint(x, y);
    }

    /// <summary>
    /// Converts X/Y in world units to a point in device independent units (or DIP or DP),
    /// respecting rotation
    /// </summary>
    /// <param name="worldX">X coordinate in world units</param>
    /// <param name="worldY">Y coordinate in world units</param>
    /// <returns>Tuple of x and y in screen coordinates</returns>
    public static (double screenX, double screenY) WorldToScreenXY(this ViewportState viewportState, double worldX, double worldY)
    {
        var (screenX, screenY) = WorldToScreenUnrotated(viewportState, worldX, worldY);

        if (viewportState.IsRotated())
        {
            var screenCenterX = viewportState.Width / 2.0;
            var screenCenterY = viewportState.Height / 2.0;
            return Rotate(-viewportState.Rotation, screenX, screenY, screenCenterX, screenCenterY);
        }

        return (screenX, screenY);

        (double x, double y) Rotate(double degrees, double x, double y, double centerX, double centerY)
        {
            // translate this point back to the center
            var newX = x - centerX;
            var newY = y - centerY;

            // rotate the values
            var p = Algorithms.RotateClockwiseDegrees(newX, newY, degrees);

            // translate back to original reference frame
            newX = p.X + centerX;
            newY = p.Y + centerY;

            return (newX, newY);
        }

        (double screenX, double screenY) WorldToScreenUnrotated(ViewportState viewportState,
            double worldX, double worldY)
        {
            var screenCenterX = viewportState.Width / 2.0;
            var screenCenterY = viewportState.Height / 2.0;
            var screenX = (worldX - viewportState.CenterX) / viewportState.Resolution + screenCenterX;
            var screenY = (viewportState.CenterY - worldY) / viewportState.Resolution + screenCenterY;
            return (screenX, screenY);
        }
    }

    /// <summary>
    /// Converts X/Y in screen pixels to a point in screen units, respecting rotation
    /// </summary>
    /// <param name="x">Screen position x coordinate</param>
    /// <param name="y">Screen position y coordinate</param>
    /// <returns>Tuple of x and y in world coordinates</returns>
    public static (double worldX, double worldY) ScreenToWorldXY(this ViewportState viewportState, double screenX, double screenY)
    {
        var screenCenterX = viewportState.Width / 2.0;
        var screenCenterY = viewportState.Height / 2.0;

        if (viewportState.IsRotated())
        {
            var screen = new MPoint(screenX, screenY).Rotate(viewportState.Rotation, screenCenterX, screenCenterY);
            screenX = screen.X;
            screenY = screen.Y;
        }

        var worldX = viewportState.CenterX + (screenX - screenCenterX) * viewportState.Resolution;
        var worldY = viewportState.CenterY - (screenY - screenCenterY) * viewportState.Resolution;
        return (worldX, worldY);
    }
}
