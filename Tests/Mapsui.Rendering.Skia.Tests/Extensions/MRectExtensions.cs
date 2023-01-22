namespace Mapsui.Rendering.Skia.Tests.Extensions;

public static class MRectExtensions
{
    public static Viewport ToViewport(this MRect rect, double width)
    {
        return new Viewport
        {
            Resolution = rect.Width / width,
            CenterX = rect.Centroid.X,
            CenterY = rect.Centroid.Y,
            Width = width,
            Height = width * (rect.Height / rect.Width)
        };
    }
}
