using BruTile;
using Mapsui.Geometries;

namespace Mapsui.Extensions
{
    public static class ExtentExtensions
    {
        public static BoundingBox? ToBoundingBox(this Extent extent)
        {
            return new BoundingBox(
                extent.MinX,
                extent.MinY,
                extent.MaxX,
                extent.MaxY);
        }

        public static MRectangle ToMRect(this Extent extent)
        {
            return new MRectangle(
                extent.MinX,
                extent.MinY,
                extent.MaxX,
                extent.MaxY);
        }
    }
}