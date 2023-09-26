using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Mapsui.Styles;
using Mapsui.Widgets;

namespace Mapsui.Samples.CustomWidget;

public class CustomWidget : IWidget
{
    private HorizontalAlignment _horizontalAlignment;
    private VerticalAlignment _verticalAlignment;
    private float _marginX = 20;
    private float _marginY = 20;
    private MRect? _envelope;
    private Color? _color;
    private int _width;
    private int _height;
    private bool _enabled = true;

    public HorizontalAlignment HorizontalAlignment
    {
        get => _horizontalAlignment;
        set
        {
            if (value == _horizontalAlignment)
            {
                return;
            }

            _horizontalAlignment = value;
            OnPropertyChanged();
        }
    }

    public VerticalAlignment VerticalAlignment
    {
        get => _verticalAlignment;
        set
        {
            if (value == _verticalAlignment)
            {
                return;
            }

            _verticalAlignment = value;
            OnPropertyChanged();
        }
    }

    public float MarginX
    {
        get => _marginX;
        set
        {
            if (value.Equals(_marginX))
            {
                return;
            }

            _marginX = value;
            OnPropertyChanged();
        }
    }

    public float MarginY
    {
        get => _marginY;
        set
        {
            if (value.Equals(_marginY))
            {
                return;
            }

            _marginY = value;
            OnPropertyChanged();
        }
    }

    public MRect? Envelope
    {
        get => _envelope;
        set
        {
            if (Equals(value, _envelope))
            {
                return;
            }

            _envelope = value;
            OnPropertyChanged();
        }
    }

    public bool HandleWidgetTouched(Navigator navigator, MPoint position)
    {
        navigator.CenterOn(0, 0);
        return true;
    }

    public Color? Color
    {
        get => _color;
        set
        {
            if (Equals(value, _color))
            {
                return;
            }

            _color = value;
            OnPropertyChanged();
        }
    }

    public int Width
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

    public int Height
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

    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (value == _enabled)
            {
                return;
            }

            _enabled = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
