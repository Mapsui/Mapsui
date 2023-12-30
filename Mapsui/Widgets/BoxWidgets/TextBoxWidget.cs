using Mapsui.Styles;

namespace Mapsui.Widgets.BoxWidgets;

/// <summary>
/// Widget displaying text in a box
/// </summary>
public class TextBoxWidget : BoxWidget
{
    private double _paddingX = 3;

    /// <summary>
    /// Padding left and right for icon inside the Widget
    /// </summary>
    public double PaddingX
    {
        get => _paddingX;
        set
        {
            if (_paddingX == value)
                return;
            _paddingX = value;
            OnPropertyChanged();
        }
    }

    private double _paddingY = 3;

    /// <summary>
    /// Padding left and right for icon inside the Widget
    /// </summary>
    public double PaddingY
    {
        get => _paddingY;
        set
        {
            if (_paddingY == value)
                return;
            _paddingY = value;
            OnPropertyChanged();
        }
    }

    private string? _text = string.Empty;

    /// <summary>
    /// Text inside of box
    /// </summary>
    public string? Text
    {
        get => _text;
        set
        {
            if (_text == value)
                return;
            _text = value;
            OnPropertyChanged();
        }
    }

    private Color _textColor = new(0, 0, 0);

    /// <summary>
    /// Text color of text inside of box
    /// </summary>
    public Color TextColor
    {
        get => _textColor;
        set
        {
            if (_textColor == value)
                return;
            _textColor = value;
            OnPropertyChanged();
        }
    }
}
