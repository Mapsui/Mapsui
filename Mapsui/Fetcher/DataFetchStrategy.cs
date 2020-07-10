// Copyright 2009 - Paul den Dulk (Geodan)
// 
// This file is part of SharpMap.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System.Collections.Generic;
using System.Linq;
using BruTile;
using Mapsui.Geometries.Utilities;

namespace Mapsui.Fetcher
{
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
}
