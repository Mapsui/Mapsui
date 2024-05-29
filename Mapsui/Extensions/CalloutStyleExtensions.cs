using Mapsui.Styles;

namespace Mapsui.Extensions;

public static class CalloutStyleExtensions
{
    public static CalloutOptions ToCalloutOptions(this CalloutStyle calloutStyle)
    {
        return new CalloutOptions(
            calloutStyle.StrokeWidth,
            calloutStyle.Padding,
            calloutStyle.RectRadius,
            calloutStyle.ArrowAlignment,
            calloutStyle.ArrowWidth,
            calloutStyle.ArrowHeight,
            calloutStyle.ArrowPosition,
            calloutStyle.ShadowWidth,
            calloutStyle.BackgroundColor,
            calloutStyle.Color
        );
    }
}
