using System;
using Mapsui.Styles;

namespace Mapsui.Rendering;

public interface IRenderService : IDisposable
{
    ILabelCache LabelCache { get; }
    ISymbolCache SymbolCache { get; }
    ITileCache TileCache { get; }
    IVectorCache VectorCache { get; }
}
