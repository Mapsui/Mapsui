using Mapsui.Logging;
using Mapsui.Utilities;

namespace Mapsui.Widgets.ButtonWidgets;

/// <summary>
/// Widget displaying a clickable hyperlink
/// </summary>
public class HyperlinkWidget : ButtonWidget
{
    private string? _url = string.Empty;

    /// <summary>
    /// URL to open when Widget is clicked
    /// </summary>
    public string? Url
    {
        get => _url;
        set
        {
            if (_url == value)
                return;
            _url = value ?? string.Empty;
            Invalidate();
        }
    }

    public override bool OnTapped(Navigator navigator, WidgetEventArgs e)
    {
        if (base.OnTapped(navigator, e))
            return true; // The user could override the behavior in the Tapped event.

        if (Url is null)
        {
            Logger.Log(LogLevel.Warning, "HyperlinkWidget: URL is not set");
            return true;
        }

        PlatformUtilities.OpenInBrowser(Url);
        return true;
    }
}
