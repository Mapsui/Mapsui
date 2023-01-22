using Mapsui.Rendering;

namespace Mapsui.Styles;

/// <summary> For Detecting Feature Size is used in Rasterizing Tiling Rendering to determine how much more Features needs to be loaded </summary>
public interface IFeatureSize
{
    /// <summary> The Feature Size is in points of the screen </summary>
    /// <param name="feature">feature to detect size</param>
    /// <param name="style">symbol style to detect size</param>
    /// <param name="symbolCache">symbol Cache</param>
    /// <returns>size in points</returns>
    double FeatureSize(IFeature feature, IStyle style, IRenderCache symbolCache);
}
