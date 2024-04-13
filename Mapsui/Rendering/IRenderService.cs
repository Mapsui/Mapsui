using System;
using Mapsui.Styles;

namespace Mapsui.Rendering;

public interface IRenderService : IDisposable
{
    IBitmapRegistry BitmapRegistry { get; }
    ILabelCache LabelCache { get; set; }
    ISymbolCache SymbolCache { get; set; }
    ITileCache TileCache { get; set; }
    IVectorCache VectorCache { get; set; }
}
