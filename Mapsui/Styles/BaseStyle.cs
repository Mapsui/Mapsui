// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

using System.Threading;

namespace Mapsui.Styles;

/// <summary>
///     Defines a style used for rendering vector data
/// </summary>
public abstract class BaseStyle : IStyle
{
    private static long _currentStyleId; // last used style id

    public BaseStyle()
    {
        GenerationId = NextId();
        MinVisible = 0;
        MaxVisible = double.MaxValue;
        Enabled = true;
        Opacity = 1f;
    }

    /// <inheritdoc />
    public long GenerationId { get; private set; }

    /// <summary>
    ///     Gets or sets the minimum zoom value where the style is applied
    /// </summary>
    public double MinVisible { get; set; }

    /// <summary>
    ///     Gets or sets the maximum zoom value where the style is applied
    /// </summary>
    public double MaxVisible { get; set; }

    /// <summary>
    ///     Gets or sets whether objects in this style is rendered or not
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    ///     Gets or sets the objects base opacity
    /// </summary>
    public float Opacity { get; set; }

    private static long NextId()
    {
        return Interlocked.Increment(ref _currentStyleId);
    }

    /// <inheritdoc />
    public virtual void Modified()
    {
        // Modified needs a new id to invalidate cached drawables.
        GenerationId = NextId();
    }
}
