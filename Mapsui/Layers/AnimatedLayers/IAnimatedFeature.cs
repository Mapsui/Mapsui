// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Mapsui.Layers.AnimatedLayers;

/// <summary>
/// Represents a feature that manages its own positional animation between two points.
/// Implement this interface on features that should be animated by a <see cref="Layer"/>
/// when its data source returns updated positions.
/// </summary>
public interface IAnimatedFeature
{
    /// <summary>
    /// Advances the feature's interpolated position based on elapsed time.
    /// </summary>
    /// <returns><c>true</c> if the animation is still running and a render update is needed;
    /// <c>false</c> if the animation has completed.</returns>
    bool UpdateAnimation();
}
