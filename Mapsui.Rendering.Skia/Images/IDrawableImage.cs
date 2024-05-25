using System;

namespace Mapsui.Rendering.Skia.Images;

public interface IDrawableImage : IDisposable
{
    public float Width { get; }
    public float Height { get; }
}
