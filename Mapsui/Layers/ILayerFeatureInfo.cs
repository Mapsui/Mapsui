using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mapsui.Layers;

/// <summary>
/// Some data source like WMS allow request for feature info. 
/// This is information not available in the primary response such as the WMS image.
/// </summary>
public interface ILayerFeatureInfo
{
    Task<IDictionary<string, IEnumerable<IFeature>>> GetFeatureInfoAsync(Viewport viewport, double screenX, double screenY);
}
