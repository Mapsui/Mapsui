using Mapsui.Rendering;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia.Drawables;

/// <summary>
/// A pre-created drawable for polygon and linestring features rendered with VectorStyle.
/// The SKPath is stored in world coordinates and transformed to screen coordinates at draw time
/// using a world-to-screen matrix (clone + transform). All paints remain in pixel/screen units.
/// </summary>
public sealed class VectorStyleDrawable : IDrawable
{
    public VectorStyleDrawable(
        double worldX,
        double worldY,
        SKPath worldPath,
        SKPaint? fillPaint,
        SKPaint? outlinePaint,
        SKPaint? linePaint,
        FillStyle fillStyle)
    {
        WorldX = worldX;
        WorldY = worldY;
        WorldPath = worldPath;
        FillPaint = fillPaint;
        OutlinePaint = outlinePaint;
        LinePaint = linePaint;
        FillStyle = fillStyle;
    }

    /// <summary>World X coordinate (geometry centroid, for IDrawable interface).</summary>
    public double WorldX { get; }

    /// <summary>World Y coordinate (geometry centroid, for IDrawable interface).</summary>
    public double WorldY { get; }

    /// <summary>Pre-created path in world coordinates. Transformed to screen coords at draw time.</summary>
    public SKPath WorldPath { get; }

    /// <summary>Pre-created fill paint for polygons, or null if no fill.</summary>
    public SKPaint? FillPaint { get; }

    /// <summary>Pre-created outline paint (polygon outline or linestring outline), or null.</summary>
    public SKPaint? OutlinePaint { get; }

    /// <summary>Pre-created line paint for linestrings, or null.</summary>
    public SKPaint? LinePaint { get; }

    /// <summary>The fill style, used to determine solid vs pattern fill drawing.</summary>
    public FillStyle FillStyle { get; }

#pragma warning disable IDISP007 // Don't dispose injected - these objects are created by and owned by this drawable
    public void Dispose()
    {
        WorldPath.Dispose();
        FillPaint?.Dispose();
        OutlinePaint?.Dispose();
        LinePaint?.Dispose();
    }
#pragma warning restore IDISP007
}
