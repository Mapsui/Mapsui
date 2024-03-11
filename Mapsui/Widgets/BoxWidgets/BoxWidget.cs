using Mapsui.Styles;

namespace Mapsui.Widgets.BoxWidgets;

/// <summary>
/// Widget displaying a box
/// </summary>
public class BoxWidget : BaseWidget
{
    private double _cornerRadius = 8;

    /// <summary>
    /// Corner radius of box
    /// </summary>
    public double CornerRadius
    {
        get => _cornerRadius;
        set
        {
            if (_cornerRadius == value)
                return;
            _cornerRadius = value;
            Invalidate();
        }
    }

    private Color? _backColor = new(255, 255, 255, 128);

    /// <summary>
    /// Background color of box
    /// </summary>
    public Color? BackColor
    {
        get => _backColor;
        set
        {
            if (_backColor == value)
                return;
            _backColor = value;
            Invalidate();
        }
    }

    private double _opacity = 0.8f;

    /// <summary>
    /// Opacity of background, frame and signs
    /// </summary>
    public double Opacity
    {
        get => _opacity;
        set
        {
            if (_opacity == value)
                return;
            _opacity = value;
            Invalidate();
        }
    }
}
