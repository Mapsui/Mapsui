using System.Collections.Generic;
using GeoJSON.Net.Feature;

namespace Mapsui.VectorTiles
{
    public interface IGeoJsonRenderer
    {
        byte[] Render(IEnumerable<FeatureCollection> featureCollections);
    }
}