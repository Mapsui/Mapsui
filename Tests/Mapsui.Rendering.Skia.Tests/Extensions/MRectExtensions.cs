namespace Mapsui.Rendering.Skia.Tests.Extensions
{
    public static class MRectExtensions
    {
        public static Viewport ToViewport(this MRect rect, double width = 400, double scaleEnvelope = 1)
        {
            return new Viewport
            {
                Resolution = rect.Width * scaleEnvelope / width,
                Center = rect.Centroid,
                Width = width,
                Height = width * (rect.Height * scaleEnvelope / (rect.Width * scaleEnvelope))
            };
        }
    }
}
