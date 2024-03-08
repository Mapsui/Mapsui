using Mapsui.Manipulations;

namespace Mapsui.Widgets;

/// <summary>
/// Widget that gets touch events
/// </summary>
public abstract class TouchableWidget : Widget, ITouchableWidget
{
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
    /// <param name="e">Arguments for this widget touch</param>
    /// <returns>True, if the Widget had handled the touch event</returns>
    public virtual bool OnTapped(Navigator navigator, ScreenPosition position, WidgetEventArgs e)
    {
        return false;
    }

    /// <summary>
    /// Function, which handles the widget touching event
    /// </summary>
    /// <param name="navigator">Navigator of MapControl</param>
    /// <param name="position">Screen position</param>
    /// <param name="e">Arguments for this widget touch</param>
    /// <returns>True, if the Widget had handled the touch event</returns>
    public virtual bool OnPointerPressed(Navigator navigator, ScreenPosition position, WidgetEventArgs e)
    {
        return false;
    }

    /// <summary>
    /// Function, which handles the widget moving event
    /// </summary>
    /// <param name="navigator">Navigator of MapControl</param>
    /// <param name="position">Screen position</param>
    /// <param name="e">Arguments for this widget touch</param>
    /// <returns>True, if the Widget had handled the touch event</returns>
    public virtual bool OnPointerMoved(Navigator navigator, ScreenPosition position, WidgetEventArgs e)
    {
        return false;
    }
}
