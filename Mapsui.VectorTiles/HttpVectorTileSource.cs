using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using BruTile;
using BruTile.Cache;
using BruTile.Web;
using Mapsui.Geometries;
using Mapsui.Rendering.Skia;

namespace Mapsui.VectorTiles
{
    public class HttpVectorTileSource : HttpTileSource
    {
        public HttpVectorTileSource(ITileSchema tileSchema, string urlFormatter, IEnumerable<string> serverNodes = null, 
            string apiKey = null, string name = null, IPersistentCache<byte[]> persistentCache = null) 
            : base(tileSchema, urlFormatter, serverNodes, apiKey, name, persistentCache, FetchTile)
        {
        }

        private static byte[] FetchTile(Uri url)
        {
            // The fetch method is injected to unzip the tile
            var gzipWebClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });
            return gzipWebClient.GetByteArrayAsync(url).Result;
        }

        public override byte[] GetTile(TileInfo tileInfo)
        {
            var tileData = base.GetTile(tileInfo);
            var features = new VectorTileParser().ToFeatures(tileInfo, tileData);
            return Rasterize(features, tileInfo);
        }

        private byte[] Rasterize(IEnumerable<Providers.Feature> features, TileInfo tileInfo)
        {
            var tileWidth = Schema.GetTileWidth(tileInfo.Index.Level);
            var tileHeight = Schema.GetTileHeight(tileInfo.Index.Level);
            var viewport = ToViewport(tileWidth, tileHeight, tileInfo.Extent.ToBoundingBox());
            return new MapRenderer().RenderToBitmapStream(viewport, features).ToArray();
        }

        private static Viewport ToViewport(int tileWidth, int tileHeight, BoundingBox boundingBox)
        {
            return new Viewport
            {
                Width = tileWidth,
                Height = tileHeight,
                Center = boundingBox.GetCentroid(),
                Resolution = boundingBox.Width / tileWidth
            };
        }
    }
}
