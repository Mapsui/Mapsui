using Mapsui.Manipulations;

namespace Mapsui.Widgets;

public interface IWidget
{
    /// <summary>
    /// Horizontal alignment of Widget
    /// </summary>
    HorizontalAlignment HorizontalAlignment { get; set; }

    /// <summary>
    /// Vertical alignment of Widget
    /// </summary>
    VerticalAlignment VerticalAlignment { get; set; }

    /// <summary>
    /// Margin outside of the widget
    /// </summary>
    MRect Margin { get; set; }

    /// <summary>
    /// Position for absolute alignment
    /// </summary>
    MPoint Position { get; set; }

    /// <summary>
    /// Width of Widget
    /// </summary>
    double Width { get; set; }

    /// <summary>
    /// Height of Widget
    /// </summary>
    double Height { get; set; }

    /// <summary>
    /// The hit box of the widget. This needs to be updated from the widget renderer.
    /// </summary>
    MRect? Envelope { get; set; }

    /// <summary>
    /// Is Widget visible on screen
    /// </summary>
    bool Enabled { get; set; }

    /// <summary>
    /// Flag for redrawing widget in the next drawing cycle
    /// </summary>
    bool NeedsRedraw { get; set; }

    /// <summary>
    /// Type of area to use for manipulation events
    /// </summary>
    WidgetAreaType TouchableArea { get; }

    /// <summary>
    /// Function, which handles the widget tapped event
    /// </summary>
    /// <param name="navigator">Navigator of MapControl</param>
    /// <param name="position">Screen position</param>
    /// <param name="e">Arguments for this widget touch</param>
    /// <returns>True, if the Widget had handled the touch event</returns>
    bool OnTapped(Navigator navigator, ScreenPosition position, WidgetEventArgs e);

    /// <summary>
    /// Function, which handles the widget pointer pressed event
    /// </summary>
    /// <param name="navigator">Navigator of MapControl</param>
    /// <param name="position">Screen position</param>
    /// <param name="e">Arguments for this widget touch</param>
    /// <returns>True, if the Widget had handled the touch event</returns>
    bool OnPointerPressed(Navigator navigator, ScreenPosition position, WidgetEventArgs e);

    /// <summary>
    /// Function, which handles the widget pointer moved event
    /// </summary>
    /// <param name="navigator">Navigator of MapControl</param>
    /// <param name="position">Screen position</param>
    /// <param name="e">Arguments for this widget touch</param>
    /// <returns>True, if the Widget had handled the touch event</returns>
    bool OnPointerMoved(Navigator navigator, ScreenPosition position, WidgetEventArgs e);

    /// <summary>
    /// Function, which handles the widget pointer released event
    /// </summary>
    /// <param name="navigator">Navigator of MapControl</param>
    /// <param name="position">Screen position</param>
    /// <param name="e">Arguments for this widget touch</param>
    /// <returns>True, if the Widget had handled the touch event</returns>
    bool OnPointerReleased(Navigator navigator, ScreenPosition position, WidgetEventArgs e);
}
