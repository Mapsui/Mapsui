using System.Collections.Generic;
using System.IO;
using System.Linq;
using BruTile;
using Mapsui.Fetcher;
using Mapsui.Providers;
using Mapsui.VectorTiles.Extensions;

namespace Mapsui.VectorTiles
{
    public class VectorTileParser : ITileParser
    {
        public IEnumerable<Feature> ToFeatures(TileInfo tileInfo, byte[] tileData)
        {
            var vectorTileLayers = Mapbox.Vector.Tile.VectorTileParser.Parse(new MemoryStream(tileData));
            var featureCollection = vectorTileLayers.Select(i => 
                i.ToMapsui(i.Name, tileInfo.Index.Col, tileInfo.Index.Row, int.Parse(tileInfo.Index.Level)));
            return featureCollection.SelectMany(i => i);
        }
    }
}