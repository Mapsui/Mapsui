using System;

namespace Mapsui.Widgets.PerformanceWidget;

/// <summary>
/// Widget which shows the drawing performance
/// </summary>
/// <remarks>
/// With this, the user could see the drawing performance on the screen.
/// It shows always the values for the last draw before this draw.
/// </remarks>
public class PerformanceWidget : Widget, ITouchableWidget
{
    public PerformanceWidget(Utilities.Performance performance)
    {
        Performance = performance;
    }

    /// <summary>
    /// Performance object which holds the values
    /// </summary>
    public Utilities.Performance Performance { get; }

    /// <summary>
    /// Event handler which is called, when the button is touched
    /// </summary>
    public event EventHandler<WidgetTouchedEventArgs>? WidgetTouched;

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
            OnPropertyChanged();
        }
    }

    public TouchableAreaType TouchableArea => TouchableAreaType.Widget;

    public bool HandleWidgetTouched(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        WidgetTouched?.Invoke(this, args);

        return args.Handled;
    }

    public bool HandleWidgetTouching(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        return false;
    }

    public bool HandleWidgetMoving(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        return false;
    }
}
