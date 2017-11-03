using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using BruTile;
using BruTile.Cache;
using BruTile.Web;
using GeoJSON.Net.Feature;
using Mapbox.Vector.Tile;
using Mapsui.Geometries;
using Mapsui.Rendering.Skia;
using Mapsui.VectorTiles.Extensions;

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
            var geoJsonFeatures = ToGeoJsonFeatures(tileInfo.Index, bytes);
            return ToImageTile(tileInfo, geoJsonFeatures);
        }

        private byte[] ToImageTile(TileInfo tileInfo, IEnumerable<FeatureCollection> geoJsonFeatures)
        {
            var tileWidth = Schema.GetTileWidth(tileInfo.Index.Level);
            var tileHeight = Schema.GetTileHeight(tileInfo.Index.Level);
            var viewport = ToViewport(tileWidth, tileHeight, tileInfo.Extent.ToBoundingBox());
            return new MapRenderer().RenderToBitmapStream(viewport, geoJsonFeatures.ToMapsui().ToList()).ToArray();
        }

        private static IEnumerable<FeatureCollection> ToGeoJsonFeatures(TileIndex tileIndex, byte[] bytes)
        {
            var layerInfos = Mapbox.Vector.Tile.VectorTileParser.Parse(new MemoryStream(bytes));
            var geoJsonFeatures = layerInfos.Select(i => i.ToGeoJSON(tileIndex.Col, tileIndex.Row, int.Parse(tileIndex.Level)));
            return geoJsonFeatures;
        }

        private static Viewport ToViewport(int canvasWidth, int canvasHeight, BoundingBox boundingBox)
        {
            return new Viewport
            {
                Width = canvasWidth,
                Height = canvasHeight,
                Center = boundingBox.GetCentroid(),
                Resolution = boundingBox.Width / canvasWidth
            };
        }
    }
}
