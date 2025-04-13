using Mapsui.Styles;

namespace Mapsui.Widgets.BoxWidgets;

/// <summary>
/// Widget displaying text in a box
/// </summary>
public class TextBoxWidget : BoxWidget
{
    /// <summary>
    /// Padding for left, top, right and bottom for text inside the Widget
    /// </summary>
    public MRect Padding { get; set; } = new(3);

    /// <summary>
    /// Text inside of box
    /// </summary>
    public string? Text { get; set; } = string.Empty;

    /// <summary>
    /// Font size of text inside of box
    /// </summary>
    public double TextSize { get; set; } = 12.0;

    /// <summary>
    /// Text color of text inside of box
    /// </summary>
    public Color TextColor { get; set; } = new(0, 0, 0);
}
