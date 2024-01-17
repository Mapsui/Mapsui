namespace Mapsui.Widgets;

public interface ITouchableWidget : IWidget
{
    /// <summary>
    /// Type of area to use for touch events
    /// </summary>
    TouchableAreaType TouchableArea { get; }

    /// <summary>
    /// Function, which handles the widget touched event
    /// </summary>
    /// <param name="navigator">Navigator of MapControl</param>
    /// <param name="position">Screen position</param>
    /// <param name="args">Arguments for this widget touch</param>
    /// <returns>True, if the Widget had handled the touch event</returns>
    bool HandleWidgetTouched(Navigator navigator, MPoint position, WidgetTouchedEventArgs args);

    /// <summary>
    /// Function, which handles the widget touching event
    /// </summary>
    /// <param name="navigator">Navigator of MapControl</param>
    /// <param name="position">Screen position</param>
    /// <param name="args">Arguments for this widget touch</param>
    /// <returns>True, if the Widget had handled the touch event</returns>
    bool HandleWidgetTouching(Navigator navigator, MPoint position, WidgetTouchedEventArgs args);

    /// <summary>
    /// Function, which handles the widget moving event
    /// </summary>
    /// <param name="navigator">Navigator of MapControl</param>
    /// <param name="position">Screen position</param>
    /// <param name="args">Arguments for this widget touch</param>
    /// <returns>True, if the Widget had handled the touch event</returns>
    bool HandleWidgetMoving(Navigator navigator, MPoint position, WidgetTouchedEventArgs args);
}
