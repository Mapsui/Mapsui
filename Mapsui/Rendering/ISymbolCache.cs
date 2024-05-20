using System;
using Mapsui.Styles;

namespace Mapsui.Rendering;

public interface ISymbolCache : IDisposable
{
    Size? GetSize(string imageSource);
    IBitmapInfo? GetOrCreate(string imageSource);
}
