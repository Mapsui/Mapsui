using Mapsui.Styles;

namespace Mapsui.Rendering;

public interface IRenderCache : ILabelCache, ISymbolCache, IVectorCache
{
    IVectorCache? VectorCache { get; set; }
    ILabelCache LabelCache { get; set; }
    ISymbolCache SymbolCache { get; set; }
}
