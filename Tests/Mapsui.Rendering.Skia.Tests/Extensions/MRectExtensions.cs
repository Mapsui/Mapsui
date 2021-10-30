namespace Mapsui.Rendering.Skia.Tests.Extensions
{
    public static class MRectExtensions
    {
        public static Viewport ToViewport(this MRect rect, double width)
        {
            return new Viewport
            {
                Resolution = rect.Width / width,
                Center = rect.Centroid,
                Width = width,
                Height = width * (rect.Height / rect.Width)
            };
        }
    }
}
