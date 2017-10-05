using System.Collections.Generic;
using System.Linq;
using BruTile;
using Mapsui.Geometries.Utilities;

namespace Mapsui.Fetcher
{
    public class MinimalFetchStrategy : IFetchStrategy
    {
        public IList<TileInfo> GetTilesWanted(ITileSchema schema, Extent extent, string levelId)
        {
            return schema.GetTileInfos(extent, levelId).OrderBy(
                t => Algorithms.Distance(extent.CenterX, extent.CenterY, t.Extent.CenterX, t.Extent.CenterY)).ToList();
        }
    }
}
