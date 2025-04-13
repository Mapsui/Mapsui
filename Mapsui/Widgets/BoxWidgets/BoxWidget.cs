using Mapsui.Styles;

namespace Mapsui.Widgets.BoxWidgets;

/// <summary>
/// Widget displaying a box
/// </summary>
public class BoxWidget : BaseWidget
{
    /// <summary>
    /// Corner radius of box
    /// </summary>
    public double CornerRadius { get; set; } = 8;

    /// <summary>
    /// Background color of box
    /// </summary>
    public Color? BackColor { get; set; } = new(255, 255, 255, 128);

    /// <summary>
    /// Opacity of background, frame and signs
    /// </summary>
    public double Opacity { get; set; } = 0.8f;
}
