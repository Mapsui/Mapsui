using System;

namespace Mapsui.Rendering;

/// <summary>
/// Represents a pre-created rendering object that can be drawn quickly on the render thread.
/// Drawables store world coordinates and pre-built platform-specific objects (e.g. SKPath, SKPaint)
/// so that the draw step on the UI thread is minimal.
/// </summary>
public interface IDrawable : IDisposable
{
    /// <summary>
    /// The world X coordinate of this drawable (used to position it via viewport transform at draw time).
    /// </summary>
    double WorldX { get; }

    /// <summary>
    /// The world Y coordinate of this drawable (used to position it via viewport transform at draw time).
    /// </summary>
    double WorldY { get; }
}
