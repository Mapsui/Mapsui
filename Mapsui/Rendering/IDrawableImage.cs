using System;

namespace Mapsui.Rendering;

public interface IDrawableImage : IDisposable
{
    public float Width { get; }
    public float Height { get; }
}
