namespace Mapsui.Widgets.ButtonWidgets;

/// <summary>
/// Widget displaying a clickable hyperlink
/// </summary>
public class HyperlinkWidget : TextButtonWidget
{
    private string _url = string.Empty;

    /// <summary>
    /// URL to open when Widget is clicked
    /// </summary>
    public string Url
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
}
