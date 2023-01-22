using BruTile;

namespace Mapsui.Tiling.Extensions;

public static class MRectExtensions
{
    public static Extent ToExtent(this MRect rect)
    {
        return new Extent(rect.MinX, rect.MinY, rect.MaxX, rect.MaxY);
    }
}
