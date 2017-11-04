using System.Collections.Generic;
using System.IO;
using BruTile;
using Mapsui.Geometries;
using Mapsui.Providers;

namespace Mapsui.Fetcher
{
    public class TileParser : ITileParser
    {
        public IEnumerable<Feature> ToFeatures(TileInfo tileInfo, byte[] tileData)
        {
            return new List<Feature> {new Feature {Geometry = ToGeometry(tileInfo, tileData)}};
        }

        private static Raster ToGeometry(TileInfo tileInfo, byte[] tileData)
        {
            // A TileSource may return a byte array that is null. This is currently only implemented
            // for MbTilesTileSource. It is to indicate that the tile is not present in the source,
            // although it should be given the tile schema. It does not mean the tile could not
            // be accessed because of some temporary reason. In that case it will throw an exception.
            // For Mapsui this is important because it will not try again and again to fetch it. 
            // Here we return the geometry as null so that it will be added to the tile cache. 
            // TileLayer.GetFeatureInView will have to return only the non null geometries.

            if (tileData == null) return null;

            return new Raster(new MemoryStream(tileData), tileInfo.Extent.ToBoundingBox());
        }

    }
}
