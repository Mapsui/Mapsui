using System;
using System.Collections.Generic;

namespace Mapsui.Layers;

/// <summary>
/// Some data source like WMS allow request for feature info. 
/// This is information not available in the primary response such as the WMS image.
/// </summary>
public interface IFeatureInfo
{
    void GetFeatureInfo(IReadOnlyViewport viewport, double x, double y, Action<IDictionary<string, IEnumerable<IFeature>>> callback);
}
