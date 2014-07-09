namespace Mapsui.Geometries
{
    public static class BoundingBoxExtensions
    {
        public static BoundingBox Copy(this BoundingBox original)
        {
            return new BoundingBox(original.MinX, original.MinY, original.MaxX, original.MaxY);
        }
    }
}
