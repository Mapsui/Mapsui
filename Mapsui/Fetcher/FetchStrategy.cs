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
using Mapsui.Utilities;
using BruTile;

namespace Mapsui.Fetcher
{
    class FetchStrategy : IFetchStrategy
    {
        private readonly Sorter sorter = new Sorter();
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
            while (level >= 0)
            {
                ////////if (!preFetchLayers.Contains(level)) continue;
                var infosOfLevel = schema.GetTilesInView(extent, level);
                infosOfLevel = PrioritizeTiles(infosOfLevel, extent.CenterX, extent.CenterY, sorter);

                foreach (TileInfo info in infosOfLevel)
                {
                    if ((info.Index.Row >= 0) && (info.Index.Col >= 0)) infos.Add(info);
                }
                level--;
            }

            return infos;
        }

        private static IEnumerable<TileInfo> PrioritizeTiles(IEnumerable<TileInfo> tiles, double centerX, double centerY, Sorter sorter)
        {
            var infos = new List<TileInfo>(tiles);

            foreach (TileInfo t in infos)
            {
                double priority = -Algorithms.Distance(centerX, centerY, t.Extent.CenterX, t.Extent.CenterY);
                t.Priority = priority;
            }

            infos.Sort(sorter);
            return infos;
        }       

        private class Sorter : IComparer<TileInfo>
        {
            public int Compare(TileInfo x, TileInfo y)
            {
                if (x.Priority > y.Priority) return -1;
                if (x.Priority < y.Priority) return 1;
                return 0;
            }
        }
    }
}
