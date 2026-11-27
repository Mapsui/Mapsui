using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia.Extensions;

internal static class SKPathBuilderExtensions
{
    /// <summary>
    /// Creates a 2D path effect that repeats the builder's path. The builder is detached, so it is empty afterwards.
    /// </summary>
    public static SKPathEffect ToPathEffect(this SKPathBuilder builder, SKMatrix matrix)
    {
        // The effect copies the path, so the detached path can be disposed right away.
        using var path = builder.Detach();
        return SKPathEffect.Create2DPath(matrix, path);
    }
}
