using BruTile;

// ReSharper disable CheckNamespace
namespace Mapsui.Geometries
{
    public static class ViewportExtensions
    {
        public static IViewport ToScaledViewport(this Viewport viewport, float scale)
        {
            var result = new Viewport(viewport);
            result.Width = result.Width * scale;
            result.Height = result.Height * scale;
            result.Resolution = result.Resolution / scale;
            return result;
        }
    }

    public static class BoundingBoxExtensions
    {
        public static BoundingBox Copy(this BoundingBox original)
        {
            return new BoundingBox(original.MinX, original.MinY, original.MaxX, original.MaxY);
        }

        public static bool IsInitialized(this BoundingBox box)
        {
            if (box == null) return false;
            if (double.IsNaN(box.Width)) return false;
            if (double.IsNaN(box.Height)) return false;

            return true;
        }

        public static Extent ToExtent(this BoundingBox boundingBox)
        {
            return new Extent(boundingBox.MinX, boundingBox.MinY, boundingBox.MaxX, boundingBox.MaxY);
        }
    }
}