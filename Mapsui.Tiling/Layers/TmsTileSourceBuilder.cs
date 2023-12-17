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
        var urlPersistentCache = persistentCache as IUrlPersistentCache;
#pragma warning disable IDISP001 // Dispose created
        var stream = await urlPersistentCache.UrlCachedStreamAsync(urlToTileMapXml);
#pragma warning restore IDISP001

        var tileSource = overrideTmsUrlWithUrlToTileMapXml
            ? TileMapParser.CreateTileSource(stream, urlToTileMapXml, persistentCache: persistentCache)
            : TileMapParser.CreateTileSource(stream, persistentCache: persistentCache);

        return tileSource!;
    }
}
