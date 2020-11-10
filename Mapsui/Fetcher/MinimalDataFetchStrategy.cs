using System.Collections.Generic;
using System.Linq;
using BruTile;
using Mapsui.Geometries.Utilities;

namespace Mapsui.Fetcher
{
    public class MinimalDataFetchStrategy : IDataFetchStrategy
    {
        public IList<TileInfo> Get(ITileSchema schema, Extent extent, int level)
        {
            return schema.GetTileInfos(extent, level).OrderBy(
                t => Algorithms.Distance(extent.CenterX, extent.CenterY, t.Extent.CenterX, t.Extent.CenterY)).ToList();
        }
    }
}
