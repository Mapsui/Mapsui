using System;

namespace Mapsui.Widgets;

/// <summary>
/// Widget that gets touch events
/// </summary>
public abstract class TouchableWidget : Widget, ITouchableWidget
{
    /// <summary>
    /// Event which is called if widget is touched
    /// </summary>
    public event EventHandler<WidgetTouchedEventArgs>? Touched;

    /// <summary>
    /// Event which is called if touching around in the widget
    /// </summary>
    public event EventHandler<WidgetTouchedEventArgs>? Touching;

    /// <summary>
    /// Event which is called if moving inside of widget
    /// </summary>
    public event EventHandler<WidgetTouchedEventArgs>? Moving;

    /// <summary>
    /// Type of area to use for touch events
    /// </summary>
    private TouchableAreaType _touchableArea = TouchableAreaType.Widget;

    /// <summary>
    /// Type of area to use for touch events
    /// </summary>
    public TouchableAreaType TouchableArea
    {
        get => _touchableArea;
        set
        {
            if (_touchableArea == value)
                return;
            _touchableArea = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Function, which handles the widget touched event
    /// </summary>
    /// <param name="navigator">Navigator of MapControl</param>
    /// <param name="position">Screen position</param>
    /// <param name="args">Arguments for this widget touch</param>
    /// <returns>True, if the Widget had handled the touch event</returns>
    public virtual bool HandleWidgetTouched(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        Touched?.Invoke(this, args);

        return args.Handled;
    }

    /// <summary>
    /// Function, which handles the widget touching event
    /// </summary>
    /// <param name="navigator">Navigator of MapControl</param>
    /// <param name="position">Screen position</param>
    /// <param name="args">Arguments for this widget touch</param>
    /// <returns>True, if the Widget had handled the touch event</returns>
    public virtual bool HandleWidgetTouching(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        Touching?.Invoke(this, args);

        return args.Handled;
    }

    /// <summary>
    /// Function, which handles the widget moving event
    /// </summary>
    /// <param name="navigator">Navigator of MapControl</param>
    /// <param name="position">Screen position</param>
    /// <param name="args">Arguments for this widget touch</param>
    /// <returns>True, if the Widget had handled the touch event</returns>
    public virtual bool HandleWidgetMoving(Navigator navigator, MPoint position, WidgetTouchedEventArgs args)
    {
        Moving?.Invoke(this, args);

        return args.Handled;
    }

}
