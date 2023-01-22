// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.IO;
using BruTile.Cache;
using BruTile.Predefined;
using BruTile.Web;
using Mapsui.Tiling.Layers;

namespace Mapsui.Tiling;

public static class OpenStreetMap
{
    public static IPersistentCache<byte[]>? DefaultCache = null;

    private static readonly BruTile.Attribution OpenStreetMapAttribution = new(
        "© OpenStreetMap contributors", "https://www.openstreetmap.org/copyright");

    public static TileLayer CreateTileLayer(string? userAgent = null)
    {
        userAgent ??= $"user-agent-of-{Path.GetFileNameWithoutExtension(System.AppDomain.CurrentDomain.FriendlyName)}";

        return new TileLayer(CreateTileSource(userAgent)) { Name = "OpenStreetMap" };
    }

    private static HttpTileSource CreateTileSource(string userAgent)
    {
        return new HttpTileSource(new GlobalSphericalMercator(),
            "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
            new[] { "a", "b", "c" }, name: "OpenStreetMap",
            attribution: OpenStreetMapAttribution, userAgent: userAgent, persistentCache: DefaultCache);
    }
}
