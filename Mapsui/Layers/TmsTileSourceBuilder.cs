using BruTile;
using BruTile.Tms;
using System;
using System.Net;
using System.Threading;
using Mapsui.Logging;

namespace Mapsui.Layers
{
    public static class TmsTileSourceBuilder
    {
        public static ITileSource Build(
            string urlToTileMapXml, 
            bool overrideTmsUrlWithUrlToTileMapXml)
        {
            var webRequest = (HttpWebRequest) WebRequest.Create(urlToTileMapXml);
            var waitHandle = new AutoResetEvent(false);
            ITileSource tileSource = null;
            Exception error = null;

            var state = new object[]
            {
                new Action<Exception>(ex =>
                {
                    error = ex;
                    waitHandle.Set();
                }),
                new Action<ITileSource>(ts =>
                {
                    tileSource = ts;
                    waitHandle.Set();
                }),
                webRequest,
                urlToTileMapXml,
                overrideTmsUrlWithUrlToTileMapXml
            };
            webRequest.BeginGetResponse(LoadTmsLayer, state);

            waitHandle.WaitOne();
            if (error != null) throw error;
            return tileSource;
        }

        public static void LoadTmsLayer(IAsyncResult result)
        {
            var state = (object[])result.AsyncState;
            var errorCallback = (Action<Exception>)state[0];

            try
            {
                var callback = (Action<ITileSource>)state[1];
                var request = (HttpWebRequest)state[2];
                var urlToTileMapXml = (string)state[3];
                var overrideTmsUrlWithUrlToTileMapXml = (bool)state[4];

                var response = request.EndGetResponse(result);
                var stream = response.GetResponseStream();
                var tileSource = overrideTmsUrlWithUrlToTileMapXml
                    ? TileMapParser.CreateTileSource(stream, urlToTileMapXml)
                    : TileMapParser.CreateTileSource(stream);
                callback(tileSource);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message, ex);
                errorCallback?.Invoke(ex);
                // else: hopelesly lost with an error on a background thread and no option to report back.
            }
        }
    }
}
