using Mapsui.Layers;
using Mapsui.Manipulations;
using Mapsui.Rendering;
using Mapsui.Styles;
using SkiaSharp;
using System.Collections.Generic;

namespace Mapsui.Experimental.Rendering.Skia.MapInfos;

/// <summary>
/// Defines a renderer that can provide map information for features at a specific screen position.
/// </summary>
public interface IMapInfoRenderer
{
    /// <summary>
    /// Gets map information for a feature at the specified screen position.
    /// </summary>
    /// <param name="canvas">The canvas used for rendering operations.</param>
    /// <param name="screenPosition">The screen position to query for map information.</param>
    /// <param name="viewport">The current viewport.</param>
    /// <param name="feature">The feature to query.</param>
    /// <param name="style">The style applied to the feature.</param>
    /// <param name="layer">The layer containing the feature.</param>
    /// <param name="renderService">The render service providing rendering capabilities.</param>
    /// <param name="margin">The margin around the screen position to consider for hit detection. Default is 0.</param>
    /// <returns>A collection of map information records for the feature at the specified position.</returns>
    IEnumerable<MapInfoRecord> GetMapInfo(
        SKCanvas canvas, ScreenPosition screenPosition, Viewport viewport, IFeature feature, IStyle style, ILayer layer, RenderService renderService, int margin = 0);
}
