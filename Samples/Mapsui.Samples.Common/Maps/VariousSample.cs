using BruTile.Predefined;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common.Maps
{
    public static class VariousSample
    {
        public static Map CreateMap()
        {
            var map = new Map();

            map.Layers.Add(new TileLayer(KnownTileSources.Create()) { Name = "OSM" });
            map.Layers.Add(LineStringSample.CreateLineStringLayer(LineStringSample.CreateLineStringStyle()));
            map.Layers.Add(PointsSample.CreateRandomPointLayerWithBitmapSymbols(map.Envelope, 10));
            map.Layers.Add(PointsSample.CreatePointLayerWithBitmapSymbolOnFeature(map.Envelope, 10));
            map.Viewport.RenderResolutionMultiplier = 2;
            return map;
        }
    }
}
