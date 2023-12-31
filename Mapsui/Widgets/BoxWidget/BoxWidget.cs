using Mapsui.Styles;

namespace Mapsui.Widgets.BoxWidget;

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
            OnPropertyChanged();
        }
    }
}
