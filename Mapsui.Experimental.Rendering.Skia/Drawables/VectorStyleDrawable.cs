using Mapsui.Rendering;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia.Drawables;

/// <summary>
/// A pre-created drawable for polygon and linestring features rendered with VectorStyle.
/// The SKPath is stored in world coordinates and transformed to screen coordinates at draw time
/// via a canvas matrix. SKPaint objects are pre-created at creation time to avoid allocations
/// during rendering. Stroke widths are stored separately and scaled by resolution at draw time.
/// </summary>
public sealed class VectorStyleDrawable : IDrawable
{
    /// <summary>
    /// Initializes a new <see cref="VectorStyleDrawable"/>.
    /// </summary>
    public VectorStyleDrawable(
        double worldX,
        double worldY,
        SKPath worldPath,
        SKPaint? fillPaint,
        FillStyle fillStyle,
        SKPaint? outlinePaint,
        float baseOutlineWidth,
        PenStyle outlinePenStyle,
        float[]? outlineDashArray,
        float outlineDashOffset,
        SKPaint? linePaint,
        float baseLineWidth,
        PenStyle linePenStyle,
        float[]? lineDashArray,
        float lineDashOffset)
    {
        WorldX = worldX;
        WorldY = worldY;
        WorldPath = worldPath;
        FillPaint = fillPaint;
        FillStyle = fillStyle;
        OutlinePaint = outlinePaint;
        BaseOutlineWidth = baseOutlineWidth;
        OutlinePenStyle = outlinePenStyle;
        OutlineDashArray = outlineDashArray;
        OutlineDashOffset = outlineDashOffset;
        LinePaint = linePaint;
        BaseLineWidth = baseLineWidth;
        LinePenStyle = linePenStyle;
        LineDashArray = lineDashArray;
        LineDashOffset = lineDashOffset;
    }

    /// <summary>World X coordinate (geometry centroid, for IDrawable interface).</summary>
    public double WorldX { get; }

    /// <summary>World Y coordinate (geometry centroid, for IDrawable interface).</summary>
    public double WorldY { get; }

    /// <summary>Pre-created path in world coordinates. Transformed to screen coords at draw time via canvas matrix.</summary>
    public SKPath WorldPath { get; }

    /// <summary>Pre-created fill paint, or null if no fill.</summary>
    public SKPaint? FillPaint { get; }

    /// <summary>The fill style (solid vs pattern).</summary>
    public FillStyle FillStyle { get; }

    /// <summary>Pre-created outline paint, or null if no outline.</summary>
    public SKPaint? OutlinePaint { get; }

    /// <summary>Base outline width before resolution scaling.</summary>
    public float BaseOutlineWidth { get; }

    /// <summary>Pen style for outline dash pattern recreation at draw time.</summary>
    public PenStyle OutlinePenStyle { get; }

    /// <summary>Custom dash array for outline (UserDefined pen style).</summary>
    public float[]? OutlineDashArray { get; }

    /// <summary>Dash offset for outline.</summary>
    public float OutlineDashOffset { get; }

    /// <summary>Pre-created line paint for linestrings, or null if no line.</summary>
    public SKPaint? LinePaint { get; }

    /// <summary>Base line width before resolution scaling.</summary>
    public float BaseLineWidth { get; }

    /// <summary>Pen style for line dash pattern recreation at draw time.</summary>
    public PenStyle LinePenStyle { get; }

    /// <summary>Custom dash array for line (UserDefined pen style).</summary>
    public float[]? LineDashArray { get; }

    /// <summary>Dash offset for line.</summary>
    public float LineDashOffset { get; }

    /// <summary>
    /// Disposes all owned native resources (WorldPath and paint objects).
    /// </summary>
    public void Dispose()
    {
#pragma warning disable IDISP007 // Don't dispose injected - these are owned by this drawable
        WorldPath.Dispose();
        FillPaint?.Dispose();
        OutlinePaint?.Dispose();
        LinePaint?.Dispose();
#pragma warning restore IDISP007
    }
}
