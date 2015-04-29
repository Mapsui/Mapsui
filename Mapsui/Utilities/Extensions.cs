using BruTile;
using Mapsui.Geometries;

namespace System.Threading.Timers
{
    public static class TimerExtensions
    {
        public static void Stop(this Timer timer)
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}


namespace Mapsui.Geometries
{
    public static class ExtentExtensions
    {
        public static Extent ToExtent(this BoundingBox boundingBox)
        {
            return new Extent(boundingBox.MinX, boundingBox.MinY, boundingBox.MaxX, boundingBox.MaxY);
        }
    }
}

namespace BruTile
{
    public static class BoundingBoxExtensions
    {
        public static BoundingBox ToBoundingBox(this Extent extent)
        {
            return new BoundingBox(
                extent.MinX,
                extent.MinY,
                extent.MaxX,
                extent.MaxY);
        }
    }
}
