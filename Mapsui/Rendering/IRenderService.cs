using System;
using Mapsui.Styles;

namespace Mapsui.Rendering;

public interface IRenderService : IDisposable
{
    ITileCache TileCache { get; }
    IVectorCache VectorCache { get; }
    ImageSourceCache ImageSourceCache { get; }
    ISpriteCache SpriteCache { get; }
}
