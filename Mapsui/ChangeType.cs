namespace Mapsui;

public enum ChangeType
{
    /// <summary>
    /// Discrete changes in Viewport state.
    /// Examples: 
    /// - Plus and minus buttons. 
    /// - Map Initialization.
    /// - Final change in an animation
    /// - Touch-up after dragging.
    /// - Final mouse wheel change
    /// </summary>
    Discrete,
    /// <summary>
    /// Continuous changes in Viewport state.
    /// Examples:
    /// - Dragging the map
    /// - During animations
    /// - Mouse wheel changes
    /// </summary>
    Continuous
}
