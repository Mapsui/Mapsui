using Mapsui.Styles;

namespace Mapsui.Rendering;

public interface IRenderService : IRenderCache, IBitmapRegistry
{
    IBitmapRegistry BitmapRegistry { get; }
}
