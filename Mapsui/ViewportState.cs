// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Mapsui;

public record class ViewportState
{
    /// <summary>
    /// The X coordinate of the center of the viewport in world coordinates
    /// </summary>
    public double CenterX { get; init; }
    /// <summary>
    /// The Y coordinate of the center of the viewport in world coordinates
    /// </summary>
    public double CenterY { get; init; }
    /// <summary>
    /// Resolution of the viewport in units per pixel
    /// </summary>
    /// <remarks>
    /// The Resolution in Mapsui is what is often called zoom level. Because Mapsui is projection independent, there 
    /// aren't any zoom levels as other map libraries have. If your map has EPSG:3857 as projection
    /// and you want to calculate the zoom, you should use the following equation
    /// 
    ///     var zoom = (float)Math.Log(78271.51696401953125 / resolution, 2);
    /// </remarks>
    public double Resolution { get; init; }
    /// <summary>
    /// Viewport rotation from True North (clockwise degrees)
    /// </summary>
    public double Rotation { get; init; }
    /// <summary>
    /// Width of viewport in screen pixels
    /// </summary>
    public double Width { get; init; }
    /// <summary>
    /// Height of viewport in screen pixels
    /// </summary>
    public double Height { get; init; }

    public ViewportState(double centerX, double centerY, double resolution, double rotation, double width, double height)
    {
        CenterX = centerX;
        CenterY = centerY;
        Resolution = resolution;
        Rotation = rotation;
        Width = width;
        Height = height;
    }
}
