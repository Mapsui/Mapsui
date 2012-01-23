// Copyright 2008 - Paul den Dulk (Geodan)
// 
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
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

using System;
using System.Collections.Generic;
using BruTile;
using BruTile.Web;
using BruTile.PreDefined;

namespace DemoConfig
{
    public class GeodanWorldWmsTileSource : ITileSource
    {
        public GeodanWorldWmsTileSource()
        {
            var schema = new SphericalMercatorInvertedWorldSchema();
            schema.Srs = "EPSG:900913";
            string url = "http://geoserver.nl/world/mapserv.cgi?map=world/world.map&VERSION=1.1.1";
            var request = new WmscRequest(new Uri(url), schema,
              new List<string>(new string[] { "world" }), new List<string>(), new Dictionary<string, string>());
            Provider = new WebTileProvider(request);
            Schema = new SphericalMercatorInvertedWorldSchema();
        }

        public ITileProvider Provider { get; private set; }
        public ITileSchema Schema { get; private set; }

    }
}
