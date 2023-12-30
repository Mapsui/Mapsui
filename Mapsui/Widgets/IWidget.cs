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
    /// Left, right or both margin depending on HorizontalAlignment
    /// </summary>
    double MarginX { get; set; }

    /// <summary>
    /// Top, bottom or both marging depending on VerticalAlignment
    /// </summary>
    double MarginY { get; set; }

    /// <summary>
    /// Position in X direction of left side for absolute alignment
    /// </summary>
    double PositionX { get; set; }

    /// <summary>
    /// Position in Y direction of top side for absolute alignment
    /// </summary>
    double PositionY { get; set; }

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
}
