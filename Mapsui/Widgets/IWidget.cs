using System;

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
    /// Type of area used for  manipulation (e.g. touch, mouse) input events.
    /// </summary>
    InputAreaType InputAreaType { get; }

    bool InputTransparent { get; init; }

    /// <summary>
    /// Event which is called if widget is tapped.
    /// </summary>
    event EventHandler<WidgetEventArgs>? Tapped;

    /// <summary>
    /// Event which is called if widget is pressed.
    /// </summary>
    event EventHandler<WidgetEventArgs>? PointerPressed;

    /// <summary>
    /// Event which is called if widget is moved.
    /// </summary>
    event EventHandler<WidgetEventArgs>? PointerMoved;

    /// <summary>
    /// Event which is called if widget is released.
    /// </summary>
    event EventHandler<WidgetEventArgs>? PointerReleased;

    /// <summary>
    /// Function, which handles the widget tapped event
    /// </summary>
    /// <param name="e">Arguments for this widget touch</param>
    /// <returns>True, if the Widget had handled the touch event</returns>
    void OnTapped(WidgetEventArgs e);

    /// <summary>
    /// Function, which handles the widget pointer pressed event
    /// </summary>
    /// <param name="e">Arguments for this widget touch</param>
    /// <returns>True, if the Widget had handled the touch event</returns>
    void OnPointerPressed(WidgetEventArgs e);

    /// <summary>
    /// Function, which handles the widget pointer moved event
    /// </summary>
    /// <param name="e">Arguments for this widget touch</param>
    /// <returns>True, if the Widget had handled the touch event</returns>
    void OnPointerMoved(WidgetEventArgs e);

    /// <summary>
    /// Function, which handles the widget pointer released event
    /// </summary>
    /// <param name="e">Arguments for this widget touch</param>
    /// <returns>True, if the Widget had handled the touch event</returns>
    void OnPointerReleased(WidgetEventArgs e);
}
