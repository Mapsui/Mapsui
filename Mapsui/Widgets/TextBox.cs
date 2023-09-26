using Mapsui.Styles;

namespace Mapsui.Widgets;

public class TextBox : Widget
{
    private int _paddingX = 3;
    private int _paddingY = 1;
    private int _cornerRadius = 8;
    private int? _height;
    private int? _width;
    private Color _textColor = new(0, 0, 0);
    private Color _backColor = new(255, 255, 255, 128);
    private string? _text;

    public int PaddingX
    {
        get => _paddingX;
        set
        {
            if (value == _paddingX)
            {
                return;
            }

            _paddingX = value;
            OnPropertyChanged();
        }
    }

    public int PaddingY
    {
        get => _paddingY;
        set
        {
            if (value == _paddingY)
            {
                return;
            }

            _paddingY = value;
            OnPropertyChanged();
        }
    }

    public int CornerRadius
    {
        get => _cornerRadius;
        set
        {
            if (value == _cornerRadius)
            {
                return;
            }

            _cornerRadius = value;
            OnPropertyChanged();
        }
    }

    public string? Text
    {
        get => _text;
        set
        {
            if (value == _text)
            {
                return;
            }

            _text = value;
            OnPropertyChanged();
        }
    }

    public Color BackColor
    {
        get => _backColor;
        set
        {
            if (Equals(value, _backColor))
            {
                return;
            }

            _backColor = value;
            OnPropertyChanged();
        }
    }

    public Color TextColor
    {
        get => _textColor;
        set
        {
            if (Equals(value, _textColor))
            {
                return;
            }

            _textColor = value;
            OnPropertyChanged();
        }
    }

    public int? Width
    {
        get => _width;
        set
        {
            if (value == _width)
            {
                return;
            }

            _width = value;
            OnPropertyChanged();
        }
    }

    public int? Height
    {
        get => _height;
        set
        {
            if (value == _height)
            {
                return;
            }

            _height = value;
            OnPropertyChanged();
        }
    }

    public override bool HandleWidgetTouched(Navigator navigator, MPoint position)
    {
        // don has any action
        return false;
    }

    public override bool Touchable => false;
}
