using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Mapsui.Widgets;

public abstract class Widget : IWidget, IWidgetTouchable, INotifyPropertyChanged
{
    private bool _enabled = true;
    private MRect? _envelope;
    private float _marginY = 2;
    private float _marginX = 2;
    private VerticalAlignment _verticalAlignment = VerticalAlignment.Bottom;
    private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Right;

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

    public float CalculatePositionX(float left, float right, float width)
    {
        switch (HorizontalAlignment)
        {
            case HorizontalAlignment.Left:
                return MarginX;

            case HorizontalAlignment.Center:
                return (right - left - width) / 2;

            case HorizontalAlignment.Right:
                return right - left - width - MarginX;
        }

        throw new ArgumentException("Unknown horizontal alignment: " + HorizontalAlignment);
    }

    public float CalculatePositionY(float top, float bottom, float height)
    {
        switch (VerticalAlignment)
        {
            case VerticalAlignment.Top:
                return MarginY;

            case VerticalAlignment.Bottom:
                return bottom - top - height - MarginY;

            case VerticalAlignment.Center:
                return (bottom - top - height) / 2;
        }

        throw new ArgumentException("Unknown vertical alignment: " + VerticalAlignment);
    }

    public abstract bool HandleWidgetTouched(Navigator navigator, MPoint position);

    public virtual bool Touchable => true;

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
