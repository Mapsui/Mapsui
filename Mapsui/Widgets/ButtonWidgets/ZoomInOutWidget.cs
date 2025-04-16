using Mapsui.Styles;

namespace Mapsui.Widgets.ButtonWidgets;

/// <summary>
/// Widget which shows two buttons (horizontal or vertical) with a "+" and a "-" sign.
/// With this, the user could zoom the map in and out.
/// 
/// Usage
/// To show a ZoomInOutWidget, add a instance of the ZoomInOutWidget to Map.Widgets by
/// 
///   map.Widgets.Add(new ZoomInOutWidget(map));
///   
/// Customize
/// Size: Height and Width of the buttons
/// Orientation: Orientation of the buttons. Could be Horizontal or Vertical. Vertical is default.
/// StrokeColor: Color of button frames
/// TextColor: Color of "+" and "-" signs
/// BackColor: Color of button background
/// Opacity: Opacity of buttons
/// ZoomFactor: Factor for changing Resolution. Default is 2;
/// </summary>
public class ZoomInOutWidget : BaseWidget
{
    /// <summary>
    /// Width and height of buttons
    /// </summary>
    public double Size { get; set; } = 40;

    /// <summary>
    /// Orientation of buttons
    /// </summary>
    public Orientation Orientation { get; set; } = Orientation.Vertical;

    /// <summary>
    /// Color of button frames
    /// </summary>
    public Color StrokeColor { get; set; } = new(192, 192, 192);

    /// <summary>
    /// Color of "+" and "-" sign
    /// </summary>
    public Color TextColor { get; set; } = new(192, 192, 192);

    /// <summary>
    /// Color of background
    /// </summary>
    public Color BackColor { get; set; } = new(224, 224, 224);

    /// <summary>
    /// Opacity of background, frame and signs
    /// </summary>
    public double Opacity { get; set; } = 0.8f;

    public override void OnTapped(WidgetEventArgs e)
    {
        base.OnTapped(e);
        if (e.Handled)
            return;

        if (Envelope == null)
            return;

        if (Orientation == Orientation.Vertical && e.ScreenPosition.Y < Envelope.MinY + Envelope.Height * 0.5 ||
            Orientation == Orientation.Horizontal && e.ScreenPosition.X < Envelope.MinX + Envelope.Width * 0.5)
        {
            e.Map.Navigator.ZoomIn(500);
        }
        else
        {
            e.Map.Navigator.ZoomOut(500);
        }

        e.Handled = true;
        return;
    }
}
