namespace Mapsui.Widgets;

public interface ITouchableWidget : IWidget
{
    /// <summary>
    /// Type of area to use for touch events
    /// </summary>
    TouchableAreaType TouchableArea { get; }

    /// <summary>
    /// Function, which handles the widget tapped event
    /// </summary>
    /// <param name="navigator">Navigator of MapControl</param>
    /// <param name="position">Screen position</param>
    /// <param name="e">Arguments for this widget touch</param>
    /// <returns>True, if the Widget had handled the touch event</returns>
    bool OnTapped(Navigator navigator, MPoint position, WidgetEventArgs e);

    /// <summary>
    /// Function, which handles the widget pointer pressed event
    /// </summary>
    /// <param name="navigator">Navigator of MapControl</param>
    /// <param name="position">Screen position</param>
    /// <param name="e">Arguments for this widget touch</param>
    /// <returns>True, if the Widget had handled the touch event</returns>
    bool OnPointerPressed(Navigator navigator, MPoint position, WidgetEventArgs e);

    /// <summary>
    /// Function, which handles the widget pointer moved event
    /// </summary>
    /// <param name="navigator">Navigator of MapControl</param>
    /// <param name="position">Screen position</param>
    /// <param name="e">Arguments for this widget touch</param>
    /// <returns>True, if the Widget had handled the touch event</returns>
    bool OnPointerMoved(Navigator navigator, MPoint position, WidgetEventArgs e);
}
