using Mapsui.Styles;

namespace Mapsui.Extensions;

public static class CalloutStyleExtensions
{
    public static CalloutBalloonDefinition ToCalloutBalloonDefinition(this CalloutStyle calloutStyle)
    {
        return calloutStyle.BalloonDefinition;
    }
}
