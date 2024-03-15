using Mapsui.Styles;

namespace Mapsui.Widgets.BoxWidgets;

/// <summary>
/// Widget displaying text in a box
/// </summary>
public class TextBoxWidget : BoxWidget
{
    private MRect _padding = new(3);

    /// <summary>
    /// Padding for left, top, right and bottom for text inside the Widget
    /// </summary>
    public MRect Padding
    {
        get => _padding;
        set
        {
            if (_padding == value)
                return;
            _padding = value;
            Invalidate();
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
            Invalidate();
        }
    }

    private double _textSize = 12.0;

    /// <summary>
    /// Font size of text inside of box
    /// </summary>
    public double TextSize
    {
        get => _textSize;
        set
        {
            if (_textSize == value)
                return;
            _textSize = value;
            Invalidate();
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
            Invalidate();
        }
    }
}
