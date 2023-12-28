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
    float MarginX { get; set; }

    /// <summary>
    /// Top, bottom or both marging depending on VerticalAlignment
    /// </summary>
    float MarginY { get; set; }

    /// <summary>
    /// Position in X direction of left side for absolute alignment
    /// </summary>
    float PositionX { get; set; }

    /// <summary>
    /// Position in Y direction of top side for absolute alignment
    /// </summary>
    float PositionY { get; set; }

    /// <summary>
    /// Width of Widget
    /// </summary>
    float Width { get; set; }

    /// <summary>
    /// Height of Widget
    /// </summary>
    float Height { get; set; }

    /// <summary>
    /// The hit box of the widget. This needs to be updated from the widget renderer.
    /// </summary>
    MRect? Envelope { get; set; }

    /// <summary>
    /// Is Widget visible on screen
    /// </summary>
    bool Enabled { get; set; }
}
