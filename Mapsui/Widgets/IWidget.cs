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
}
