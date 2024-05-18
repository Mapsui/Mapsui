using System;
using Mapsui.Styles;

namespace Mapsui.Rendering;

public interface ISymbolCache : IDisposable
{
    Size? GetSize(string bitmapPath);
    IBitmapInfo GetOrCreate(string bitmapPath);
}
