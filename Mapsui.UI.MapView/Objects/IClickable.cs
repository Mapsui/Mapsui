using System;

namespace Mapsui.UI.Objects;

/// <summary>
/// Interface for objects that are clickable
/// </summary>
internal interface IClickable
{
    /// <summary>
    /// Gets a value indicating whether this <see cref="T:Mapsui.UI.Objects.IClickable"/> is clickable.
    /// </summary>
    /// <value><c>true</c> if is clickable; otherwise, <c>false</c>.</value>
    bool IsClickable { get; }

    /// <summary>
    /// Handle click event
    /// </summary>
    /// <param name="e">Event args for drawable clicked</param>
    void HandleClicked(DrawableClickedEventArgs e);

    /// <summary>
    /// Get information, when this object is clicked
    /// </summary>
    event EventHandler<DrawableClickedEventArgs> Clicked;
}
