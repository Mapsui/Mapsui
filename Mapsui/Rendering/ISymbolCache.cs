using System;
using Mapsui.Styles;

namespace Mapsui.Rendering;

public interface ISymbolCache : IDisposable
{
    Size? GetSize(int bitmapId); // perhaps use a tuple in C#7

    IBitmapInfo GetOrCreate(int bitmapID);
}
