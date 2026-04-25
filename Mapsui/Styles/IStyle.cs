// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

namespace Mapsui.Styles;

/// <summary>
/// Defines an interface for defining layer styles
/// </summary>
public interface IStyle
{
    /// <summary>
    /// Gets the generation identifier for this style instance. This changes when
    /// <see cref="Modified"/> is called to signal that cached drawables should be invalidated.
    /// </summary>
    long GenerationId { get; }

    /// <summary>
    /// Gets or sets the minimum zoom value where the style is applied
    /// </summary>
    double MinVisible { get; set; }

    /// <summary>
    /// Gets or sets the maximum zoom value where the style is applied
    /// </summary>
    double MaxVisible { get; set; }

    /// <summary>
    /// Gets or sets whether objects in this style is rendered or not
    /// </summary>
    bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the objects overall opacity
    /// </summary>
    float Opacity { get; set; }

    /// <summary>
    /// Signals that the style has been modified. This updates the <see cref="GenerationId"/>
    /// so that cached drawables using this style can be invalidated.
    /// </summary>
    void Modified();
}
