using System.Collections.Generic;
using System.Linq;
using BruTile;
using Mapsui.Utilities;

namespace Mapsui.Tiling.Fetcher;

public class MinimalDataFetchStrategy : IDataFetchStrategy
{
    public IList<TileInfo> Get(ITileSchema schema, Extent extent, int level)
    {
        return schema.GetTileInfos(extent, level).OrderBy(
            t => Algorithms.Distance(extent.CenterX, extent.CenterY, t.Extent.CenterX, t.Extent.CenterY)).ToList();
    }
}
