namespace Mapsui.UI;

public class ViewportLimiterWithoutLimits : BaseViewportLimiter
{
    public override ViewportState Limit(ViewportState viewportState)
    {
        return viewportState;
    }
}
