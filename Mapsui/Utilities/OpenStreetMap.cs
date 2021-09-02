using BruTile.Predefined;
using BruTile.Web;
using Mapsui.Layers;

namespace Mapsui.Utilities
{
    public static class OpenStreetMap
    {
        private const string DefaultUserAgent = "Default Mapsui user-agent";
        private static readonly BruTile.Attribution OpenStreetMapAttribution = new BruTile.Attribution(
            "© OpenStreetMap contributors", "https://www.openstreetmap.org/copyright");

        public static TileLayer CreateTileLayer()
        {
            return new TileLayer(CreateTileSource()) { Name = "OpenStreetMap" };
        }

        private static HttpTileSource CreateTileSource(string userAgent = DefaultUserAgent)
        {
            return new HttpTileSource(new GlobalSphericalMercator(),
                "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
                new[] { "a", "b", "c" }, name: "OpenStreetMap",
                attribution: OpenStreetMapAttribution, userAgent: userAgent);
        }
    }
}