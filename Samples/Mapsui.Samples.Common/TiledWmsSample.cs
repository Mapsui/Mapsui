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
using System.Linq;
using BruTile;
using BruTile.Web;
using BruTile.Predefined;
using BruTile.Wmsc;
using Mapsui.Layers;

namespace Mapsui.Samples.Common
{
    /// <summary>
    /// And ordinary WMS service called through a tiled schema (WMS-C) 
    /// </summary>
    public static class TiledWmsSample
    {
        public static ILayer CreateLayer()
        { 
            return new TileLayer(new GeodanWorldWmsTileSource()) { Name = "WMS called as WMSC" };
        }
    }

    public class GeodanWorldWmsTileSource : ITileSource
    {
        public GeodanWorldWmsTileSource()
        {
            var schema = new GlobalSphericalMercator(YAxis.TMS) { Srs = "EPSG:900913"};
            Provider = new HttpTileProvider(CreateWmsRequest(schema));
            Schema = schema;
        }

        public byte[] GetTile(TileInfo tileInfo)
        {
            return Provider.GetTile(tileInfo);
        }
        
        private static WmscRequest CreateWmsRequest(ITileSchema schema)
        {
            const string url = "http://geoserver.nl/world/mapserv.cgi?map=world/world.map&VERSION=1.1.1";
            return new WmscRequest(new Uri(url), schema, new[] {"world"}.ToList(), new string[0].ToList());
        }

        public ITileProvider Provider { get; }
        public ITileSchema Schema { get; }

        public string Name => "GeodanWorldWmsTileSource";
    }
}
