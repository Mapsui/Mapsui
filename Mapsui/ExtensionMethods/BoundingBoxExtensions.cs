namespace Mapsui.Geometries
{
    public static class BoundingBoxExtensions
    {
        public static BoundingBox Copy(this BoundingBox original)
        {
            return new BoundingBox(original.MinX, original.MinY, original.MaxX, original.MaxY);
        }

        public static bool IsInitialized(this BoundingBox box)
        {
            if (box == null) return false; // 'this' can be null: http://stackoverflow.com/questions/847209/in-c-what-happens-when-you-call-an-extension-method-on-a-null-object
            if (double.IsNaN(box.Width)) return false;
            if (double.IsNaN(box.Height)) return false;
            return true;
        }
    }
}