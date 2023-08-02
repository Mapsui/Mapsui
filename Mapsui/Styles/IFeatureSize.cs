using Mapsui.Rendering;

namespace Mapsui.Styles;

/// <summary> For Detecting Feature Size is used in Rasterizing Tiling Rendering to determine how much more Features needs to be loaded </summary>
public interface IFeatureSize
{
    /// <summary>
    /// True if it needs a feature to determine the Feature Size. For Example Symbols have always
    /// the same Size and therefore don't need a Feature.
    /// Labels have variable Size depending on the Text and therefore need a feature to determine the size
    /// </summary>
    bool NeedsFeature { get; }

    /// <summary> The Feature Size is in points of the screen </summary>
    /// <param name="style">symbol style to detect size</param>
    /// <param name="symbolCache">symbol Cache</param>
    /// <param name="feature">feature to detect size</param>
    /// <returns>size in points</returns>
    double FeatureSize(IStyle style, IRenderCache symbolCache, IFeature? feature = null);
}
