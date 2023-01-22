using Mapsui.Styles;

namespace Mapsui.Rendering;

public interface ISymbolCache
{
    Size? GetSize(int bitmapId); // perhaps use a tuple in C#7

    IBitmapInfo GetOrCreate(int bitmapID);
}
