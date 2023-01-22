using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BruTile;
using BruTile.Cache;
using BruTile.Tms;
using Mapsui.Cache;
using Mapsui.Extensions;
using Mapsui.Logging;

#pragma warning disable IDE0079 // Justification: There is an error in the tool, removing the suppression below introduced a warning.

namespace Mapsui.Tiling.Layers;

public static class TmsTileSourceBuilder
{
    public static async Task<ITileSource> BuildAsync(string urlToTileMapXml,
        bool overrideTmsUrlWithUrlToTileMapXml,
        IPersistentCache<byte[]>? persistentCache = null)
    {
        var urlPersitentCache = persistentCache as IUrlPersistentCache;
#pragma warning disable IDISP001 // Dispose created
        var stream = await urlPersitentCache.UrlCachedStreamAsync(urlToTileMapXml);
#pragma warning restore IDISP001

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
