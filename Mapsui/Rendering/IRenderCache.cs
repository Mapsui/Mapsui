using System;

namespace Mapsui.Rendering;

public interface IRenderCache : IDisposable
{
    ILabelCache LabelCache { get; set; }
    ISymbolCache SymbolCache { get; set; }
    ITileCache TileCache { get; set; }
    IVectorCache VectorCache { get; set; }
}
