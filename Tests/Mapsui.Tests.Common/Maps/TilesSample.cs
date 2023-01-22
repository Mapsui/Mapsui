using System.Collections.Generic;
using System.Threading.Tasks;
using BruTile;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.Tiling.Extensions;
using Mapsui.UI;

namespace Mapsui.Tests.Common.Maps;

public class TilesSample : ISample
{
    public string Name => "Tiles";
    public string Category => "Tests";

    public async Task<Map> CreateMapAsync()
    {
        var layer = await CreateLayerAsync();

        var map = new Map
        {
            BackColor = Color.FromString("WhiteSmoke"),
            Home = n => n.NavigateToFullEnvelope()
        };

        map.Layers.Add(layer);

        return map;
    }

    private static async Task<MemoryLayer> CreateLayerAsync()
    {
        var tileIndexes = new[]
        {
            new TileIndex(0, 0, 1),
            new TileIndex(1, 0, 1),
            new TileIndex(0, 1, 1),
            new TileIndex(1, 1, 1)
        };

        return new MemoryLayer
        {
            Features = await TileIndexToFeaturesAsync(tileIndexes, new SampleTileSource()),
            Name = "Tiles",
            Style = new RasterStyle()
        };
    }

    private static async Task<List<RasterFeature>> TileIndexToFeaturesAsync(TileIndex[] tileIndexes, ITileSource tileSource)
    {
        var features = new List<RasterFeature>();
        foreach (var tileIndex in tileIndexes)
        {
            var tileInfo = new TileInfo
            {
                Index = tileIndex,
                Extent = TileTransform.TileToWorld(
                    new TileRange(tileIndex.Col, tileIndex.Row), tileIndex.Level, tileSource.Schema)
            };

            var raster = new MRaster(await tileSource.GetTileAsync(tileInfo), tileInfo.Extent.ToMRect());
            features.Add(new RasterFeature(raster));
        }
        return features;
    }
}
