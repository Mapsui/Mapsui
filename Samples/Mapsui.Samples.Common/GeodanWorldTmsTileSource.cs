// Copyright 2008 - Paul den Dulk (Geodan)
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

using System;
using System.Collections.Generic;
using BruTile;
using BruTile.Tms;
using BruTile.Web;
using BruTile.Predefined;

namespace Mapsui.Samples.Common
{
    public class GeodanWorldTmsTileSource : ITileSource
    {
        public GeodanWorldTmsTileSource()
        {
            const string url = "http://geoserver.nl/tiles/tilecache.aspx/1.0.0/world_GM/";
            var parameters = new Dictionary<string, string>();
            parameters.Add("seriveparam", "world_GM");
            parameters.Add("uid", "4c6b3b161be3a2eb513b66b09a70f18d");
            var request = new TmsRequest(new Uri(url), "png", parameters);
            Provider = new WebTileProvider(request);
            Schema = new SphericalMercatorWorldSchema();
        }

        public ITileProvider Provider { get; private set; }
        public ITileSchema Schema { get; private set; }
        public string Name { get; private set; }
    }
}
