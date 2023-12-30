using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Mapsui.Widgets;

public abstract class Widget : IWidget, INotifyPropertyChanged
{
    private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Right;

    /// <summary>
    /// Horizontal alignment of Widget
    /// </summary>
    public HorizontalAlignment HorizontalAlignment
    {
        get => _horizontalAlignment;
        set
        {
            if (_horizontalAlignment == value)
                return;
            _horizontalAlignment = value;
            OnPropertyChanged();
        }
    }

    private VerticalAlignment _verticalAlignment { get; set; } = VerticalAlignment.Bottom;

    /// <summary>
    /// Vertical alignment of Widget
    /// </summary>
    public VerticalAlignment VerticalAlignment
    {
        get => _verticalAlignment;
        set
        {
            if (_verticalAlignment == value)
                return;
            _verticalAlignment = value;
            OnPropertyChanged();
        }
    }

    private double _marginX = 2;

    /// <summary>
    /// Left, right or both margin depending on HorizontalAlignment
    /// </summary>
    public double MarginX 
    { 
        get => _marginX;
        set
        {
            if (_marginX == value)
                return;
            _marginX = value;
            OnPropertyChanged();
        }
    }

    private double _marginY = 2;

    /// <summary>
    /// Top, bottom or both marging depending on VerticalAlignment
    /// </summary>
    public double MarginY
    {
        get => _marginY;
        set
        {
            if (_marginY == value)
                return;
            _marginY = value;
            OnPropertyChanged();
        }
    }

    private double _positionX = 0;

    /// <summary>
    /// Position in X direction of left side for absolute alignment
    /// </summary>
    public double PositionX
    {
        get => _positionX;
        set
        {
            if (_positionX == value)
                return;
            _positionX = value;
            OnPropertyChanged();
        }
    }

    private double _positionY = 0;

    /// <summary>
    /// Position in Y direction of left side for absolute alignment
    /// </summary>
    public double PositionY
    {
        get => _positionY;
        set
        {
            if (_positionY == value)
                return;
            _positionY = value;
            OnPropertyChanged();
        }
    }

    private double _width;

    /// <summary>
    /// Width of Widget
    /// </summary>
    public double Width
    {
        get => _width;
        set
        {
            if (_width == value)
                return;
            _width = value;
            OnPropertyChanged();
        }
    }

    private double _height;

    /// <summary>
    /// Height of Widget
    /// </summary>
    public double Height
    {
        get => _height;
        set
        {
            if (_height == value)
                return;
            _height = value;
            OnPropertyChanged();
        }
    }

    private MRect? _envelope;

    /// <summary>
    /// Envelope of Widget
    /// </summary>
    public MRect? Envelope
    {
        get => _envelope;
        set
        {
            if (_envelope == value)
                return;
            _envelope = value;
            OnPropertyChanged();
        }
    }

    private bool _enabled = true;

    /// <summary>
    /// Is Widget visible on screen
    /// </summary>
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value)
                return;
            _enabled = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Event handler which is called, when a property changes
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual void OnPropertyChanged([CallerMemberName] string name = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private double CalculatePositionX(double left, double right, double width) => HorizontalAlignment switch
    {
        HorizontalAlignment.Left => MarginX,
        HorizontalAlignment.Center => (right - left - width) / 2f,
        HorizontalAlignment.Right => right - left - width - MarginX,
        _ => throw new ArgumentException("Unknown horizontal alignment: " + HorizontalAlignment)
    };

    private double CalculatePositionY(double top, double bottom, double height) => VerticalAlignment switch
    {
        VerticalAlignment.Top => MarginY,
        VerticalAlignment.Bottom => bottom - top - height - MarginY,
        VerticalAlignment.Center => (bottom - top - height) / 2f,
        _ => throw new ArgumentException("Unknown vertical alignment: " + VerticalAlignment)
    };
}
