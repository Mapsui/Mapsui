using System.IO;
using BruTile.Predefined;
using BruTile.Web;
using Mapsui.Layers;

namespace Mapsui.Utilities
{
    public static class OpenStreetMap
    {
        private static readonly BruTile.Attribution OpenStreetMapAttribution = new(
            "© OpenStreetMap contributors", "https://www.openstreetmap.org/copyright");

        public static TileLayer CreateTileLayer(string userAgent = null)
        {
            userAgent ??= $"user-agent-of-{Path.GetFileNameWithoutExtension(System.AppDomain.CurrentDomain.FriendlyName)}";

            return new TileLayer(CreateTileSource(userAgent)) { Name = "OpenStreetMap" };
        }

        private static HttpTileSource CreateTileSource(string userAgent)
        {
            return new HttpTileSource(new GlobalSphericalMercator(),
                "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
                new[] { "a", "b", "c" }, name: "OpenStreetMap",
                attribution: OpenStreetMapAttribution, userAgent: userAgent);
        }
    }
}