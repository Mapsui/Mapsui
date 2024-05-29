using Mapsui.Styles;

namespace Mapsui.Extensions;

public static class CalloutStyleExtensions
{
    public static CalloutBalloonStyle ToCalloutBalloonStyle(this CalloutStyle calloutStyle)
    {
        return new CalloutBalloonStyle(
            calloutStyle.StrokeWidth,
            calloutStyle.Padding,
            calloutStyle.RectRadius,
            calloutStyle.TailAlignment,
            calloutStyle.TailWidth,
            calloutStyle.TailHeight,
            calloutStyle.TailPosition,
            calloutStyle.ShadowWidth,
            calloutStyle.BackgroundColor,
            calloutStyle.Color
        );
    }
}
