using System;

namespace Mapsui.Widgets;

public abstract class BaseWidget : IWidget
{
    /// <summary>
    /// Type of area to use for touch events. The default is WidgetArea. This needs to be set to 
    /// 'Map' in the constructor if widget want to receive manipulation events from all over the map.
    /// </summary>
    public InputAreaType InputAreaType { get; init; } = InputAreaType.Widget;

    /// <summary>
    /// Horizontal alignment of Widget
    /// </summary>
    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Right;

    /// <summary>
    /// Vertical alignment of Widget
    /// </summary>
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Bottom;

    /// <summary>
    /// Margin outside of the widget
    /// </summary>
    public MRect Margin { get; set; } = new(2);

    /// <summary>
    /// Position for absolute alignment
    /// </summary>
    public MPoint Position { get; set; } = new MPoint(0, 0);

    /// <summary>
    /// Width of Widget
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Height of Widget
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// Envelope of Widget
    /// </summary>
    public MRect? Envelope { get; set; }

    /// <summary>
    /// Is Widget visible on screen
    /// </summary>
    public bool Enabled { get; set; } = true;

    public bool InputTransparent { get; init; }

    /// <summary>
    /// This is an init only property to allow Tapped event initialization within an expression body method.
    /// </summary>
    public EventHandler<WidgetEventArgs>? WithTappedEvent { init { Tapped += value; } }

    /// <summary>
    /// This is an init only property to allow PointerPressed event initialization within an expression body method.
    /// </summary>
    public EventHandler<WidgetEventArgs>? WithPointerPressedEvent { init { PointerPressed += value; } }

    /// <summary>
    /// This is an init only property to allow PointerMoved event initialization within an expression body method.
    /// </summary>
    public EventHandler<WidgetEventArgs>? WithPointerMovedEvent { init { PointerMoved += value; } }

    /// <summary>
    /// This is an init only property to allow PointerReleased event initialization within an expression body method.
    /// </summary>
    public EventHandler<WidgetEventArgs>? WithPointerReleased { init { PointerReleased += value; } }

    /// <summary>
    /// Event which is called if widget is tapped.
    /// </summary>
    public event EventHandler<WidgetEventArgs>? Tapped;

    /// <summary>
    /// Event which is called if widget is pressed.
    /// </summary>
    public event EventHandler<WidgetEventArgs>? PointerPressed;

    /// <summary>
    /// Event which is called if widget is moved.
    /// </summary>
    public event EventHandler<WidgetEventArgs>? PointerMoved;

    /// <summary>
    /// Event which is called if widget is released.
    /// </summary>
    public event EventHandler<WidgetEventArgs>? PointerReleased;

    public void UpdateEnvelope(double maxWidth, double maxHeight, double screenWidth, double screenHeight)
    {
        var minX = CalculatePositionX(0, screenWidth, maxWidth);
        var minY = CalculatePositionY(0, screenHeight, maxHeight);
        var maxX = HorizontalAlignment == HorizontalAlignment.Stretch ? screenWidth - Margin.Right : minX + maxWidth;
        var maxY = VerticalAlignment == VerticalAlignment.Stretch ? screenHeight - Margin.Bottom : minY + maxHeight;

        Envelope = new MRect(minX, minY, maxX, maxY);
    }

    /// <inheritdoc/>
    public virtual void OnTapped(WidgetEventArgs e)
    {
        Tapped?.Invoke(this, e);
    }

    /// <inheritdoc/>
    public virtual void OnPointerPressed(WidgetEventArgs e)
    {
        PointerPressed?.Invoke(this, e);
    }

    /// <inheritdoc/>
    public virtual void OnPointerMoved(WidgetEventArgs e)
    {
        PointerMoved?.Invoke(this, e);
    }

    /// <inheritdoc/>
    public virtual void OnPointerReleased(WidgetEventArgs e)
    {
        PointerReleased?.Invoke(this, e);
    }

    private double CalculatePositionX(double left, double right, double width) => HorizontalAlignment switch
    {
        HorizontalAlignment.Left => Margin.Left,
        HorizontalAlignment.Center => (right - left - width) / 2f,
        HorizontalAlignment.Right => right - left - width - Margin.Right,
        HorizontalAlignment.Stretch => Margin.Left,
        HorizontalAlignment.Absolute => Position.X,
        _ => throw new ArgumentException("Unknown horizontal alignment: " + HorizontalAlignment)
    };

    private double CalculatePositionY(double top, double bottom, double height) => VerticalAlignment switch
    {
        VerticalAlignment.Top => Margin.Top,
        VerticalAlignment.Bottom => bottom - top - height - Margin.Bottom,
        VerticalAlignment.Center => (bottom - top - height) / 2f,
        VerticalAlignment.Stretch => Margin.Top,
        VerticalAlignment.Absolute => Position.Y,
        _ => throw new ArgumentException("Unknown vertical alignment: " + VerticalAlignment)
    };
}
