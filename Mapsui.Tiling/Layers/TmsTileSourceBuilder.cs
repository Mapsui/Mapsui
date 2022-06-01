using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using BruTile;
using BruTile.Cache;
using BruTile.Tms;
using Mapsui.Cache;
using Mapsui.Extensions;
using Mapsui.Logging;

#pragma warning disable IDE0079 // Justification: There is an error in the tool, removing the suppression below introduced a warning.
#pragma warning disable SYSLIB0014 // Justification: This is old code that is hardly used. Only replace WebRequest with HttpClient if we have use case

namespace Mapsui.Tiling.Layers
{
    public static class TmsTileSourceBuilder
    {
        public static ITileSource Build(string urlToTileMapXml,
            bool overrideTmsUrlWithUrlToTileMapXml, 
            IPersistentCache<byte[]>? persistentCache = null)
        {
            Exception? error = null;
            var bytes = (persistentCache as IUrlPersistentCache)?.Find(urlToTileMapXml);
            if (bytes == null)
            {
                var webRequest = (HttpWebRequest)WebRequest.Create(urlToTileMapXml);
                using var waitHandle = new AutoResetEvent(false);

                var state = new object[]
                {
                    new Action<Exception>(ex =>
                    {
                        error = ex;
                        waitHandle.Set();
                    }),
                    new Action<byte[]?>(ts =>
                    {
                        bytes = ts;
                        waitHandle.Set();
                    }),
                    webRequest
                };
                webRequest.BeginGetResponse(LoadTmsLayer, state);

                waitHandle.WaitOne();
            }

            if (error is not null) throw error;

            if (bytes is null) 
                throw new HttpRequestException($"Could not retrieve data from {urlToTileMapXml}");

            var stream = new MemoryStream(bytes);
            var tileSource = overrideTmsUrlWithUrlToTileMapXml
                ? TileMapParser.CreateTileSource(stream, urlToTileMapXml, persistentCache: persistentCache)
                : TileMapParser.CreateTileSource(stream, persistentCache: persistentCache);

            return tileSource!;
        }

        private static void LoadTmsLayer(IAsyncResult result)
        {
            var state = (object[]?)result.AsyncState;
            if (state == null)
                return;
            
            var errorCallback = (Action<Exception>)state[0];

            try
            {
                var callback = (Action<byte[]>)state[1];
                var request = (HttpWebRequest)state[2];

                using var response = request.EndGetResponse(result);
                using var stream = response.GetResponseStream();
                callback(stream.ToBytes());
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message, ex);
                errorCallback?.Invoke(ex);
            }
        }
    }
}
