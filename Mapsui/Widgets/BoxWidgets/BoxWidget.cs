using Mapsui.Styles;

namespace Mapsui.Widgets.BoxWidgets;

/// <summary>
/// Widget displaying a box
/// </summary>
public class BoxWidget : Widget
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
            OnPropertyChanged();
        }
    }

    private Color? _backColor = Color.Transparent;

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
            OnPropertyChanged();
        }
    }

    private double _opacity = 0.0f;

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
            OnPropertyChanged();
        }
    }
}
