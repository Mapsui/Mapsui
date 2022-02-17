using BruTile;

namespace Mapsui.Extensions
{
    public static class ExtentExtensions
    {
        public static MRect ToMRect(this Extent extent)
        {
            return new MRect(
                extent.MinX,
                extent.MinY,
                extent.MaxX,
                extent.MaxY);
        }
    }
}