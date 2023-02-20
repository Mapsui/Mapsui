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

    public static ViewportState operator +(ViewportState a, ViewportState b)
    {
        return a with
        {
            CenterX = a.CenterX + b.CenterX,
            CenterY = a.CenterY + b.CenterY,
            Resolution = a.Resolution + b.Resolution,
            Rotation = a.Rotation + b.Rotation,
            Width = a.Width + b.Width,
            Height = a.Height + b.Height
        };
    }

    public static ViewportState operator -(ViewportState a, ViewportState b)
    {
        return a with
        {
            CenterX = a.CenterX - b.CenterX,
            CenterY = a.CenterY - b.CenterY,
            Resolution = a.Resolution - b.Resolution,
            Rotation = a.Rotation - b.Rotation,
            Width = a.Width - b.Width,
            Height = a.Height - b.Height
        };
    }

    public static ViewportState operator *(ViewportState v, double m)
    {
        return v with
        {
            CenterX = v.CenterX * m,
            CenterY = v.CenterY * m,
            Resolution = v.Resolution * m,
            Rotation = v.Rotation * m,
            Width = v.Width * m,
            Height = v.Height * m
        };
    }

    public static ViewportState operator /(ViewportState v, double d)
    {
        return v with
        {
            CenterX = v.CenterX / d,
            CenterY = v.CenterY / d,
            Resolution = v.Resolution / d,
            Rotation = v.Rotation / d,
            Width = v.Width / d,
            Height = v.Height / d
        };
    }
}
