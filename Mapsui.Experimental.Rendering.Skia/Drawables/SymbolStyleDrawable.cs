using Mapsui.Rendering;
using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia.Drawables;

/// <summary>
/// A pre-created drawable for SymbolStyle features. Holds the SKPath and SKPaint objects
/// that were created on a background thread, ready for fast drawing on the UI thread.
/// </summary>
public sealed class SymbolStyleDrawable : IDrawable
{
    /// <summary>
    /// Initializes a new <see cref="SymbolStyleDrawable"/>.
    /// </summary>
    /// <param name="worldX">The world X coordinate of the symbol.</param>
    /// <param name="worldY">The world Y coordinate of the symbol.</param>
    /// <param name="path">The pre-created path for the symbol shape.</param>
    /// <param name="fillPaint">The fill paint, or null if fill is not visible.</param>
    /// <param name="outlinePaint">The outline paint, or null if outline is not visible.</param>
    /// <param name="symbolScale">The symbol scale factor.</param>
    /// <param name="symbolRotation">The symbol rotation in degrees.</param>
    /// <param name="rotateWithMap">Whether the symbol rotates with the map.</param>
    /// <param name="offsetX">The absolute X offset.</param>
    /// <param name="offsetY">The absolute Y offset (sign already flipped).</param>
    /// <param name="relativeOffsetX">The relative X offset.</param>
    /// <param name="relativeOffsetY">The relative Y offset.</param>
    /// <param name="opacity">The combined opacity (layer × style).</param>
    public SymbolStyleDrawable(
        double worldX,
        double worldY,
        SKPath path,
        SKPaint? fillPaint,
        SKPaint? outlinePaint,
        float symbolScale,
        double symbolRotation,
        bool rotateWithMap,
        float offsetX,
        float offsetY,
        float relativeOffsetX,
        float relativeOffsetY,
        float opacity)
    {
        WorldX = worldX;
        WorldY = worldY;
        Path = path;
        FillPaint = fillPaint;
        OutlinePaint = outlinePaint;
        SymbolScale = symbolScale;
        SymbolRotation = symbolRotation;
        RotateWithMap = rotateWithMap;
        OffsetX = offsetX;
        OffsetY = offsetY;
        RelativeOffsetX = relativeOffsetX;
        RelativeOffsetY = relativeOffsetY;
        Opacity = opacity;
    }

    /// <inheritdoc />
    public double WorldX { get; }

    /// <inheritdoc />
    public double WorldY { get; }

    /// <summary>Pre-created path for the symbol shape (circle, rectangle, triangle).</summary>
    public SKPath Path { get; }

    /// <summary>Pre-created fill paint, or null if fill is not visible.</summary>
    public SKPaint? FillPaint { get; }

    /// <summary>Pre-created outline paint, or null if outline is not visible.</summary>
    public SKPaint? OutlinePaint { get; }

    /// <summary>The symbol scale factor.</summary>
    public float SymbolScale { get; }

    /// <summary>The symbol rotation in degrees.</summary>
    public double SymbolRotation { get; }

    /// <summary>Whether the symbol rotates with the map.</summary>
    public bool RotateWithMap { get; }

    /// <summary>The absolute X offset.</summary>
    public float OffsetX { get; }

    /// <summary>The absolute Y offset (note: sign is already flipped).</summary>
    public float OffsetY { get; }

    /// <summary>The combined opacity (layer * style).</summary>
    public float Opacity { get; }

    /// <summary>The relative X offset (resolved to absolute at draw time based on symbol size).</summary>
    public float RelativeOffsetX { get; }

    /// <summary>The relative Y offset (resolved to absolute at draw time based on symbol size).</summary>
    public float RelativeOffsetY { get; }

#pragma warning disable IDISP007 // Don't dispose injected - these objects are created by and owned by this drawable
    public void Dispose()
    {
        Path.Dispose();
        FillPaint?.Dispose();
        OutlinePaint?.Dispose();
    }
#pragma warning restore IDISP007
}
