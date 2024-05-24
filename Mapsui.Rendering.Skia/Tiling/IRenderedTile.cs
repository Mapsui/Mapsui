using System;

namespace Mapsui.Rendering.Skia.Tiling;

public interface IRenderedTile : IDisposable
{
    long IterationUsed { get; set; }
}
