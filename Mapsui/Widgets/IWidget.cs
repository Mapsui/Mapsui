namespace Mapsui.Widgets;

public interface IWidget
{
    HorizontalAlignment HorizontalAlignment { get; set; }
    VerticalAlignment VerticalAlignment { get; set; }
    float MarginX { get; set; }
    float MarginY { get; set; }

    /// <summary>
    /// The hit box of the widget. This needs to be updated from the widget renderer.
    /// </summary>
    MRect? Envelope { get; set; }

    /// <summary>
    /// Function, which is called, when a Widget is hidden
    /// </summary>
    /// <param name="navigator">Navigator of MapControl</param>
    /// <param name="position">Screen position</param>
    /// <returns>True, if the Widget had handled the touch event</returns>
    bool HandleWidgetTouched(INavigator navigator, MPoint position);

    bool Enabled { get; set; }
}
