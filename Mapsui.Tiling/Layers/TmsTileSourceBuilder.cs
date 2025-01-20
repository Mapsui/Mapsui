using System.IO;
using System.Threading.Tasks;
using BruTile;
using BruTile.Cache;
using BruTile.Tms;
using Mapsui.Cache;
using Mapsui.Extensions;

#pragma warning disable IDE0079 // Justification: There is an error in the tool, removing the suppression below introduced a warning.

namespace Mapsui.Tiling.Layers;

public static class TmsTileSourceBuilder
{
    public static async Task<ITileSource> BuildAsync(string urlToTileMapXml,
        bool overrideTmsUrlWithUrlToTileMapXml,
        IPersistentCache<byte[]>? persistentCache = null)
    {
        var urlPersistentCache = persistentCache as IUrlPersistentCache;
        var bytes = await urlPersistentCache.UrlCachedStreamAsync(urlToTileMapXml);
        using var stream = new MemoryStream(bytes);

        var tileSource = overrideTmsUrlWithUrlToTileMapXml
            ? TileMapParser.CreateTileSource(stream, urlToTileMapXml, persistentCache: persistentCache)
            : TileMapParser.CreateTileSource(stream, persistentCache: persistentCache);

        return tileSource!;
    }
}
