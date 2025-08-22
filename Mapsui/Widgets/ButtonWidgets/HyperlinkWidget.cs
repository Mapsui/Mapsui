using Mapsui.Logging;
using Mapsui.Utilities;

namespace Mapsui.Widgets.ButtonWidgets;

/// <summary>
/// Widget displaying a clickable hyperlink
/// </summary>
public class HyperlinkWidget : ButtonWidget
{
    /// <summary>
    /// URL to open when Widget is clicked
    /// </summary>
    public string? Url { get; set; } = string.Empty;

    public override void OnTapped(WidgetEventArgs e)
    {
        base.OnTapped(e);
        if (e.Handled)
            return;

        if (Url is null)
        {
            Logger.Log(LogLevel.Warning, "HyperlinkWidget: URL is not set");
            e.Handled = true;
            return;
        }

        PlatformUtilities.OpenInBrowser(Url);
        e.Handled = true;
    }
}
