using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia.SkiaStyles;

public record CalloutBalloonBounds(
    double Bottom,
    double Left,
    double Top,
    double Right,
    SKPoint TailStart,
    SKPoint TailEnd,
    SKPoint TailTip);
