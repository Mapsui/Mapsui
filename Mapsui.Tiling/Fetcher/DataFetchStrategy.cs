// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Paul den Dulk (Geodan) as part of SharpMap

using System.Collections.Generic;
using System.Linq;
using BruTile;
using Mapsui.Utilities;

namespace Mapsui.Tiling.Fetcher;

public class DataFetchStrategy : IDataFetchStrategy
{
    private readonly int _maxLevelsUp;

    public DataFetchStrategy(int maxLevelsUp = int.MaxValue)
    {
        _maxLevelsUp = maxLevelsUp;
    }

    public IList<TileInfo> Get(ITileSchema schema, Extent extent, int level)
    {
        var tileInfos = new List<TileInfo>();
        // Iterating through all levels from current to zero. If lower levels are
        // not available the renderer can fall back on higher level tiles. 
        var resolution = schema.Resolutions[level].UnitsPerPixel;
        var levels = schema.Resolutions.Where(k => k.Value.UnitsPerPixel >= resolution).OrderBy(x => x.Value.UnitsPerPixel).ToList();

        var counter = 0;
        foreach (var l in levels)
        {
            if (counter > _maxLevelsUp) break;

            var tileInfosForLevel = schema.GetTileInfos(extent, l.Key).OrderBy(
                t => Algorithms.Distance(extent.CenterX, extent.CenterY, t.Extent.CenterX, t.Extent.CenterY));

            tileInfos.AddRange(tileInfosForLevel);
            counter++;
        }

        return tileInfos;
    }
}
