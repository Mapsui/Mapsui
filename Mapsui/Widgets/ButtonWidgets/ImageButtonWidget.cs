using Mapsui.Styles;
using Mapsui.Widgets.BoxWidgets;

namespace Mapsui.Widgets.ButtonWidgets;

/// <summary>
/// Widget that shows a button with an icon
/// </summary>
public class ImageButtonWidget : BoxWidget, IHasImage
{
    public ImageButtonWidget() : base()
    {
        BackColor = Color.Transparent;
    }

    /// <summary>
    /// Padding left and right for icon inside the Widget
    /// </summary>
    public MRect Padding { get; set; } = new(0);

    /// <summary>
    /// The image to show as button
    /// </summary>
    public Image? Image { get; set; }

    /// <summary>
    /// Rotation of the SVG image
    /// </summary>
    public double Rotation { get; set; }
}
