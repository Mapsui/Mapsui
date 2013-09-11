// Copyright 2009 - Paul den Dulk (Geodan)
// 
// This file is part of Mapsui.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Mapsui is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with Mapsui; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System.Collections.Generic;
using System.Linq;
using Mapsui.Utilities;
using BruTile;

namespace Mapsui.Fetcher
{
    class FetchStrategy : IFetchStrategy
    {
        HashSet<int> preFetchLayers;
        public static HashSet<int> GetPreFetchLevels(int min, int max)
        {
            var preFetchLayers = new HashSet<int>();
            int level = min;
            var step = 1;
            while (level <= max)
            {
                preFetchLayers.Add(level);
                level += step;
                step++;
            }
            return preFetchLayers;
        }

        public IList<TileInfo> GetTilesWanted(ITileSchema schema, Extent extent, int level)
        {
            //line below only works properly of this instance is always called with the the resolutions. Think of something better
            if (preFetchLayers == null) preFetchLayers = GetPreFetchLevels(0, schema.Resolutions.Count - 1);

            IList<TileInfo> infos = new List<TileInfo>();
            // Iterating through all levels from current to zero. If lower levels are
            // not availeble the renderer can fall back on higher level tiles. 
            var levels = schema.Resolutions.Keys.Where(k => k <= level).OrderByDescending(x => x);

            foreach (var lvl in levels)
            {
                var tileInfos = schema.GetTilesInView(extent, lvl).OrderBy(
                    t => Algorithms.Distance(extent.CenterX, extent.CenterY, t.Extent.CenterX, t.Extent.CenterY));

                foreach (TileInfo info in tileInfos.Where(info => (info.Index.Row >= 0) && (info.Index.Col >= 0)))
                {
                    infos.Add(info);
                }
            }

            return infos;
        }
    }
}
