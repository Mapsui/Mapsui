using System;
using Mapsui.Styles;

namespace Mapsui.Rendering;

public interface IRenderCache : ILabelCache, ISymbolCache, IVectorCache, ITileCache, IDisposable 
{
    ILabelCache LabelCache { get; set; }
    ISymbolCache SymbolCache { get; set; }
    ITileCache TileCache { get; set; }
}

public interface IRenderCache<TPath, TPaint> : IRenderCache, IVectorCache<TPath, TPaint>
{
    IVectorCache<TPath, TPaint> VectorCache { get; set; }
}
