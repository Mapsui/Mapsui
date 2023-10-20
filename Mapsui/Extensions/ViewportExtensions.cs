using System;
using Mapsui.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Extensions;

public static class ViewportExtensions
{
    /// <summary>
    /// True if Width and Height are not zero
    /// </summary>
    public static bool HasSize(this Viewport viewport) =>
        viewport.Width > 0 && viewport.Height > 0;

    /// <summary>Transforms the MRect from world coordinates to screen coordinates. Note, that
    /// an MRect always represents and unrotated box. If the Viewport is rotated this will result
    /// in an unrotated box that encompasses the rotated transformation.</summary>
    /// <param name="viewport">Viewport</param>
    /// <param name="rect">The MRect to transform</param>
    /// <returns>Transformed rect</returns>
    public static MRect WorldToScreen(this Viewport viewport, MRect rect)
    {
        if (!viewport.IsRotated()) // Checking on IsRotated for performance reasons
        {
            var min = viewport.WorldToScreen(rect.Min);
            var max = viewport.WorldToScreen(rect.Max);
            return new MRect(min.X, min.Y, max.X, max.Y);
        }

        // In case of the rotated viewport all four coordinates
        // are transformed and the min and max x/y of these are
        // taken to form the new box. In this case the result is
        // not a real transformation because an MRect can not be
        // rotated.
        var screenPoints = new List<MPoint>
        {
            viewport.WorldToScreen(rect.BottomLeft),
            viewport.WorldToScreen(rect.BottomRight),
            viewport.WorldToScreen(rect.TopRight),
            viewport.WorldToScreen(rect.TopLeft)
        };

        var minx = screenPoints.Select(p => p.X).Min();
        var miny = screenPoints.Select(p => p.Y).Min();
        var maxx = screenPoints.Select(p => p.X).Max();
        var maxy = screenPoints.Select(p => p.Y).Max();

        return new MRect(minx, miny, maxx, maxy);
    }

    /// <summary>
    /// IsRotated is true, when viewport displays map rotated
    /// </summary>
    public static MSection ToSection(this Viewport viewport)
    {
        return new MSection(viewport.ToExtent(), viewport.Resolution);
    }

    /// <summary>
    /// IsRotated is true, when viewport displays map rotated
    /// </summary>
    public static bool IsRotated(this Viewport viewport) =>
        !double.IsNaN(viewport.Rotation) && Math.Abs(viewport.Rotation % 360) > Constants.Epsilon;

    /// <summary>
    /// Calculates extent from the viewport.
    /// </summary>
    /// <remarks>
    /// This MRect is horizontally and vertically aligned, even if the viewport
    /// is rotated. So this MRect perhaps contain parts, that are not visible.
    /// </remarks>
    public static MRect ToExtent(this Viewport viewport)
    {
        // todo: Find out how this method relates to Viewport.UpdateExtent 

        // calculate the window extent 
        var halfSpanX = viewport.Width * viewport.Resolution * 0.5;
        var halfSpanY = viewport.Height * viewport.Resolution * 0.5;
        var minX = viewport.CenterX - halfSpanX;
        var minY = viewport.CenterY - halfSpanY;
        var maxX = viewport.CenterX + halfSpanX;
        var maxY = viewport.CenterY + halfSpanY;

        if (!viewport.IsRotated())
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
            return windowExtent.Rotate(-viewport.Rotation, viewport.CenterX, viewport.CenterY).ToBoundingBox();
        }
    }

    /// <summary>
    /// Converts X/Y in world units to a point in device independent unit (or DIP or DP),
    /// respecting rotation
    /// </summary>
    /// <param name="worldPosition">Coordinate in world units</param>
    /// <returns>MPoint in screen pixels</returns>  
    public static MPoint WorldToScreen(this Viewport viewport, MPoint worldPosition)
    {
        return viewport.WorldToScreen(worldPosition.X, worldPosition.Y);
    }

    /// <summary>
    /// Converts a point in screen pixels to one in screen units, respecting rotation
    /// </summary>
    /// <param name="screenPosition">Coordinate in screen units</param>
    /// <returns>MPoint in world units</returns>
    /// <inheritdoc />
    public static MPoint ScreenToWorld(this Viewport viewport, MPoint screenPosition)
    {
        return viewport.ScreenToWorld(screenPosition.X, screenPosition.Y);
    }

    /// <summary>
    /// Converts X/Y in screen pixels to a point in screen units, respecting rotation
    /// </summary>
    /// <param name="x">Screen position x coordinate</param>
    /// <param name="y">Screen position y coordinate</param>
    /// <returns>MPoint in world units</returns>
    public static MPoint ScreenToWorld(this Viewport viewport, double screenX, double screenY)
    {
        var (x, y) = viewport.ScreenToWorldXY(screenX, screenY);
        return new MPoint(x, y);
    }

    /// <summary>
    /// Converts X/Y in world units to a point in device independent units (or DIP or DP),
    /// respecting rotation
    /// </summary>
    /// <param name="worldX">X coordinate in world units</param>
    /// <param name="worldY">Y coordinate in world units</param>
    /// <returns>MPoint in screen pixels</returns>
    public static MPoint WorldToScreen(this Viewport viewport, double worldX, double worldY)
    {
        var (x, y) = viewport.WorldToScreenXY(worldX, worldY);
        return new MPoint(x, y);
    }

    /// <summary>
    /// Converts X/Y in world units to a point in device independent units (or DIP or DP),
    /// respecting rotation
    /// </summary>
    /// <param name="worldX">X coordinate in world units</param>
    /// <param name="worldY">Y coordinate in world units</param>
    /// <returns>Tuple of x and y in screen coordinates</returns>
    public static (double screenX, double screenY) WorldToScreenXY(this Viewport viewport, double worldX, double worldY)
    {
        var (screenX, screenY) = WorldToScreenUnrotated(viewport, worldX, worldY);

        if (viewport.IsRotated())
        {
            var screenCenterX = viewport.Width / 2.0;
            var screenCenterY = viewport.Height / 2.0;
            return Rotate(-viewport.Rotation, screenX, screenY, screenCenterX, screenCenterY);
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

        (double screenX, double screenY) WorldToScreenUnrotated(Viewport viewport, double worldX, double worldY)
        {
            var screenCenterX = viewport.Width / 2.0;
            var screenCenterY = viewport.Height / 2.0;
            var screenX = (worldX - viewport.CenterX) / viewport.Resolution + screenCenterX;
            var screenY = (viewport.CenterY - worldY) / viewport.Resolution + screenCenterY;
            return (screenX, screenY);
        }
    }

    /// <summary>
    /// Converts X/Y in screen pixels to a point in screen units, respecting rotation
    /// </summary>
    /// <param name="x">Screen position x coordinate</param>
    /// <param name="y">Screen position y coordinate</param>
    /// <returns>Tuple of x and y in world coordinates</returns>
    public static (double worldX, double worldY) ScreenToWorldXY(this Viewport viewport, double screenX, double screenY)
    {
        var screenCenterX = viewport.Width / 2.0;
        var screenCenterY = viewport.Height / 2.0;

        if (viewport.IsRotated())
        {
            var screen = new MPoint(screenX, screenY).Rotate(viewport.Rotation, screenCenterX, screenCenterY);
            screenX = screen.X;
            screenY = screen.Y;
        }

        var worldX = viewport.CenterX + (screenX - screenCenterX) * viewport.Resolution;
        var worldY = viewport.CenterY - (screenY - screenCenterY) * viewport.Resolution;
        return (worldX, worldY);
    }
}
