using BruTile.Predefined;
using Mapsui.Layers;

namespace Mapsui.Samples.Common
{
    public static class VariousSample
    {
        public static Map CreateMap()
        {
            var map = new Map();

            map.Layers.Add(new TileLayer(KnownTileSources.Create()) { Name = "OSM" });
            map.Layers.Add(LineStringSample.CreateLineStringLayer(LineStringSample.CreateLineStringStyle()));
            map.Layers.Add(PointsSample.CreateRandomPointLayer(map.Envelope,
                style: PointsSample.CreateBitmapStyle("Mapsui.Samples.Common.Images.ic_place_black_24dp.png")));
            map.Layers.Add(PointsSample.CreateBitmapPointLayer());
            map.Viewport.RenderResolutionMultiplier = 2;

            return map;
        }
    }
}
