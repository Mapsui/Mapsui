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

using System.Diagnostics;
using BruTile;
using BruTile.Cache;
using BruTile.FileSystem;
using BruTile.Predefined;

namespace Mapsui.Samples.Common.Desktop
{
    public class MapTilerTileSource : ITileSource
    {
        public MapTilerTileSource()
        {
            Schema = GetTileSchema();
            Provider = GetTileProvider();
        }

        public ITileSchema Schema { get; private set; }
        public string Name { get; private set; }
        public ITileProvider Provider { get; private set; }

        public byte[] GetTile(TileInfo tileInfo)
        {
            return Provider.GetTile(tileInfo);
        }
        
        public static ITileProvider GetTileProvider()
        {
            return new FileTileProvider(new FileCache(GetAppDir() + "\\GeoData\\TrueMarble", "png"));
        }

        public static ITileSchema GetTileSchema()
        {
            var schema = new GlobalSphericalMercator(YAxis.TMS);
            schema.Resolutions.Clear();
            schema.Resolutions["0"] = new Resolution("0", 156543.033900000, 256, 256);
            schema.Resolutions["1"] = new Resolution("1", 78271.516950000, 256, 256);
            return schema;
        }

        private static string GetAppDir()
        {
            return System.IO.Path.GetDirectoryName(
              System.Reflection.Assembly.GetEntryAssembly().GetModules()[0].FullyQualifiedName);
        }
    }
}
