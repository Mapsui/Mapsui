namespace Mapsui.Manipulations;

public enum GestureType
{
    /// <summary>
    /// First up after a down on nearly the same position.
    /// </summary>
    SingleTap,
    /// <summary>
    /// Two SingleTaps on nearly the same position within a certain time period.
    /// </summary>
    DoubleTap,
    /// <summary>
    /// // Previously down on nearly the same position during some specific period.
    /// </summary>
    LongPress,
    /// <summary>
    /// Previously up on other position.
    /// </summary>
    Hover,
    /// <summary>
    /// Previously down on other position.
    /// </summary>
    Drag,
    /// <summary>
    /// First up.
    /// </summary>
    Release,
    /// <summary>
    /// First down.
    /// </summary>
    Press,
}
