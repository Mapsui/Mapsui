using System;
using System.Net;
using System.Net.Http;
using BruTile;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Rendering;

namespace Mapsui.VectorTiles
{
    public class VectorTileLayer : GenericTileLayer<VectorTileParser>
    {
        public VectorTileLayer(Func<ITileSource> tileSourceInitializer) : base(tileSourceInitializer) { }

        public VectorTileLayer(ITileSource source = null, int minTiles = 200, int maxTiles = 300, int maxRetries = 2,
            IFetchStrategy fetchStrategy = null,
            ITileRenderStrategy tileRenderStrategy = null, int minExtraTiles = -1, int maxExtraTiles = -1) :
            base(source, minTiles, maxTiles, maxRetries, fetchStrategy, tileRenderStrategy, minExtraTiles,
                maxExtraTiles)
        {
        }

        public static byte[] FetchTile(Uri url)
        {
            var gzipWebClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });
            return gzipWebClient.GetByteArrayAsync(url).Result;
        }
    }
}