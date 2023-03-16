namespace Mapsui.Rendering.Skia.Tests.Extensions;

public static class MRectExtensions
{
    public static ViewportState ToViewportState(this MRect rect, double width)
    {
        return new ViewportState(
            rect.Centroid.X,
            rect.Centroid.Y,
            rect.Width / width,
            0,
            width,
            width * (rect.Height / rect.Width)
        );
    }
}
