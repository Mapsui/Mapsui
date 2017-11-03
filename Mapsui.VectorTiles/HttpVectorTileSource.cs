using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using BruTile;
using BruTile.Cache;
using BruTile.Web;
using Mapbox.Vector.Tile;

namespace Mapsui.VectorTiles
{
    public class HttpVectorTileSource : HttpTileSource
    {
        public HttpVectorTileSource(ITileSchema tileSchema, string urlFormatter, IEnumerable<string> serverNodes = null, string apiKey = null, string name = null, IPersistentCache<byte[]> persistentCache = null) 
            : base(tileSchema, urlFormatter, serverNodes, apiKey, name, persistentCache, FetchTile)
        {
        }

        private static byte[] FetchTile(Uri url)
        {
            var gzipWebClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });
            return gzipWebClient.GetByteArrayAsync(url).Result;
        }

        public override byte[] GetTile(TileInfo tileInfo)
        {
            var bytes = base.GetTile(tileInfo);
            var index = tileInfo.Index;
            var layerInfos = Mapbox.Vector.Tile.VectorTileParser.Parse(new MemoryStream(bytes));
            var tileWidth = Schema.GetTileWidth(tileInfo.Index.Level);
            var tileHeight = Schema.GetTileHeight(tileInfo.Index.Level);
            var geoJSONRenderer = GetGeoJsonRenderer(tileInfo, tileWidth, tileHeight);
            return geoJSONRenderer.Render(layerInfos.Select(i => i.ToGeoJSON(index.Col, index.Row, int.Parse(index.Level))));
        }

        private IGeoJsonRenderer GetGeoJsonRenderer(TileInfo tileInfo, int tileWidth, int tileHeight)
        {
             return new GeoJsonToSkiaRenderer(tileWidth, tileHeight, ToGeoJSONArray(tileInfo.Extent));
        }

        private static double[] ToGeoJSONArray(Extent extent)
        {
            // GeoJSON.NET has no class for bounding boxes. It just holds them in a double array. 
            // The spec says it should first the lowest and then all the highest values for all axes:
            // http://geojson.org/geojson-spec.html#bounding-boxes
            return new [] {extent.MinX, extent.MinY, extent.MaxX, extent.MaxY };

        }
    }
}
