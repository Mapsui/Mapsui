using System.Collections.Generic;
using SkiaSharp;
using VexTile.Renderer.Mvt.AliFlux.Drawing;

namespace Mapsui.Experimental.VectorTiles.VexTileCopies;

/// <summary>
/// Canvas interface for vector tile rendering.
/// Implementations own their drawing surface and reusable resources.
/// </summary>
public interface ICanvas
{
    bool ClipOverflow { get; set; }

    SKColor BackgroundColor { get; }

    void DrawBackground(SKColor color);

    void DrawLineString(List<Point> geometry, Brush style);

    void DrawPolygon(List<Point> geometry, Brush style, SKColor? background);

    void DrawPoint(Point geometry, Brush style);

    void DrawText(Point geometry, Brush style);

    void DrawTextOnPath(List<Point> geometry, Brush style);

    void DrawImage(byte[] imageData, Brush style);

    void DrawUnknown(List<List<Point>> geometry, Brush style);

    byte[] ToPngByteArray(int quality = 80);
}
