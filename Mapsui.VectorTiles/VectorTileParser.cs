using System.Collections.Generic;
using System.IO;
using System.Linq;
using BruTile;
using Mapbox.Vector.Tile;
using Mapsui.Fetcher;
using Mapsui.VectorTiles.Extensions;
using Feature = Mapsui.Providers.Feature;

namespace Mapsui.VectorTiles
{
    public class VectorTileParser : ITileParser
    {
        public IEnumerable<Feature> Parse(TileInfo tileInfo, byte[] tileData)
        {
            var layerInfos = Mapbox.Vector.Tile.VectorTileParser.Parse(new MemoryStream(tileData));
            var featureCollection = layerInfos.Select(i => 
                i.ToGeoJSON(tileInfo.Index.Col, tileInfo.Index.Row, int.Parse(tileInfo.Index.Level)));
            return featureCollection.ToMapsui();
        }
    }
}
