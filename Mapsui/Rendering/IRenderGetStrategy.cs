using System.Collections.Generic;
using BruTile;
using BruTile.Cache;
using Mapsui.Geometries;
using Mapsui.Providers;

namespace Mapsui.Rendering
{
    public interface IRenderGetStrategy
    {
        IList<IFeature> GetFeatures(BoundingBox box, double resolution, ITileSchema schema,
            ITileCache<Feature> memoryCache);
    }
}
