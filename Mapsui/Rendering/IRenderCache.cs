using System;
using Mapsui.Styles;

namespace Mapsui.Rendering;

public interface IRenderCache : ILabelCache, ISymbolCache, IVectorCache, ITileCache, IDisposable
{
    IVectorCache? VectorCache { get; set; }
    ILabelCache LabelCache { get; set; }
    ISymbolCache SymbolCache { get; set; }
    ITileCache TileCache { get; set; }
}
