using Mapsui.Rendering;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia.Drawables;

/// <summary>
/// A pre-created drawable for polygon and linestring features rendered with VectorStyle.
/// The SKPath is stored in world coordinates and transformed to screen coordinates at draw time
/// via a canvas matrix. Paint objects are created locally at draw time from the stored style
/// parameters, avoiding native-object lifecycle issues between background and UI threads.
/// </summary>
public sealed class VectorStyleDrawable : IDrawable
{
    public VectorStyleDrawable(
        double worldX,
        double worldY,
        SKPath worldPath,
        Brush? brush,
        float fillOpacity,
        double viewportRotation,
        SKImage? bitmapFillImage,
        Pen? outlinePen,
        float? outlineWidthOverride,
        float outlineOpacity,
        Pen? linePen,
        float lineOpacity,
        FillStyle fillStyle)
    {
        WorldX = worldX;
        WorldY = worldY;
        WorldPath = worldPath;
        Brush = brush;
        FillOpacity = fillOpacity;
        ViewportRotation = viewportRotation;
        BitmapFillImage = bitmapFillImage;
        OutlinePen = outlinePen;
        OutlineWidthOverride = outlineWidthOverride;
        OutlineOpacity = outlineOpacity;
        LinePen = linePen;
        LineOpacity = lineOpacity;
        FillStyle = fillStyle;
    }

    /// <summary>World X coordinate (geometry centroid, for IDrawable interface).</summary>
    public double WorldX { get; }

    /// <summary>World Y coordinate (geometry centroid, for IDrawable interface).</summary>
    public double WorldY { get; }

    /// <summary>Pre-created path in world coordinates. Transformed to screen coords at draw time via canvas matrix.</summary>
    public SKPath WorldPath { get; }

    /// <summary>The Mapsui Brush for polygon fills, or null if no fill.</summary>
    public Brush? Brush { get; }

    /// <summary>Combined opacity (layer * style) for the fill paint.</summary>
    public float FillOpacity { get; }

    /// <summary>Viewport rotation at creation time (used for BitmapRotated fill).</summary>
    public double ViewportRotation { get; }

    /// <summary>Pre-extracted SKImage for Bitmap/BitmapRotated fills (cache-owned, not disposed by this drawable).</summary>
    public SKImage? BitmapFillImage { get; }

    /// <summary>The Mapsui Pen for polygon outlines or linestring outer strokes, or null.</summary>
    public Pen? OutlinePen { get; }

    /// <summary>Explicit width override for linestring outlines (line width + 2 * outline width).</summary>
    public float? OutlineWidthOverride { get; }

    /// <summary>Combined opacity for the outline paint.</summary>
    public float OutlineOpacity { get; }

    /// <summary>The Mapsui Pen for linestrings, or null.</summary>
    public Pen? LinePen { get; }

    /// <summary>Combined opacity for the line paint.</summary>
    public float LineOpacity { get; }

    /// <summary>The fill style, used to determine solid vs pattern fill drawing.</summary>
    public FillStyle FillStyle { get; }

    public void Dispose()
    {
        // Only WorldPath is a native object owned by this drawable.
        // BitmapFillImage is cache-owned and must not be disposed here.
#pragma warning disable IDISP007 // Don't dispose injected - WorldPath is created by and owned by this drawable
        WorldPath.Dispose();
#pragma warning restore IDISP007
    }
}
