using Mapsui.Styles;

namespace Mapsui.Rendering
{
    public interface ISymbolCache
    {
        Size GetSize(int bitmapId); // perhaps use a tuble in C#7
    }
}
