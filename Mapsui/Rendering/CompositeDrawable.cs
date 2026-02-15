using System;
using System.Collections.Generic;

namespace Mapsui.Rendering;

/// <summary>
/// A drawable that wraps multiple child drawables into a single cache entry.
/// Used when a single feature produces multiple drawable objects (e.g. a
/// GeometryCollection with polygons and lines, or a feature with multiple coordinates).
/// The composite uses the centroid of all children's world positions.
/// </summary>
public sealed class CompositeDrawable : IDrawable
{
    /// <summary>
    /// The child drawables. Owned by this composite and disposed with it.
    /// </summary>
    public IReadOnlyList<IDrawable> Children { get; }

    /// <inheritdoc />
    public double WorldX { get; }

    /// <inheritdoc />
    public double WorldY { get; }

    /// <summary>
    /// Creates a new <see cref="CompositeDrawable"/> from the given children.
    /// </summary>
    /// <param name="children">The child drawables. Must not be empty.</param>
    public CompositeDrawable(IReadOnlyList<IDrawable> children)
    {
        Children = children ?? throw new ArgumentNullException(nameof(children));

        // Use first child's position as representative (avoids allocation for averaging)
        if (children.Count > 0)
        {
            WorldX = children[0].WorldX;
            WorldY = children[0].WorldY;
        }
    }

    /// <summary>
    /// Disposes all child drawables.
    /// </summary>
    public void Dispose()
    {
        foreach (var child in Children)
        {
            child.Dispose();
        }
    }
}
