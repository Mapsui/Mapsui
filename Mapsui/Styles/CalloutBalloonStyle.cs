namespace Mapsui.Styles;

public record CalloutBalloonStyle(
    double StrokeWidth,
    MRect Padding,
    double RectRadius,
    TailAlignment TailAlignment,
    double TailWidth,
    double TailHeight,
    double TailPosition,
    double ShadowWidth,
    Color BackgroundColor,
    Color Color)
{ }
