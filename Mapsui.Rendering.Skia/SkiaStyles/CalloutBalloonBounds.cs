using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaStyles;

public record CalloutBalloonBounds(
    double Bottom,
    double Left,
    double Top,
    double Right,
    SKPoint TailStart,
    SKPoint TailEnd,
    SKPoint TailTip);
