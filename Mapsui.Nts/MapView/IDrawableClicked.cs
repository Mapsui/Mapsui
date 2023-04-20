

// ReSharper disable once CheckNamespace
namespace Mapsui;

public interface IDrawableClicked
{
    /// <summary>
    /// Point of click in EPSG:4326 coordinates
    /// </summary>
    Position Point { get; }

    /// <summary>
    /// Number of taps
    /// </summary>
    int NumOfTaps { get; }

    /// <summary>
    /// Flag, if this event was handled
    /// </summary>
    /// <value><c>true</c> if handled; otherwise, <c>false</c>.</value>
    bool Handled { get; set; }
}
