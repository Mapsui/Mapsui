using BruTile;
using BruTile.Predefined;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Tiling.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Tests;

public class TilesSample : ISample
{
    public string Name => "Tiles";
    public string Category => "Tests";

    public async Task<Map> CreateMapAsync()
    {
        var map = new Map
        {
            BackColor = Color.WhiteSmoke,
        };

        map.Navigator.ZoomToPanBounds();

        map.Layers.Add(await CreateLayerAsync());

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

    private static async Task<List<RasterFeature>> TileIndexToFeaturesAsync(TileIndex[] tileIndexes, SampleTileSource tileSource)
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

            var tileAsync = await tileSource.GetTileAsync(tileInfo);
            if (tileAsync == null)
                continue;

            var raster = new MRaster(tileAsync, tileInfo.Extent.ToMRect());
            features.Add(new RasterFeature(raster));
        }
        return features;
    }

    public class SampleTileSource : ILocalTileSource
    {
        private readonly IDictionary<TileIndex, byte[]> _dictionary = new Dictionary<TileIndex, byte[]>();

        public SampleTileSource() => AddTiles();

        public ITileSchema Schema { get; } = GetTileSchema();
        public string Name { get; } = "TileSource";
        public Attribution Attribution { get; } = new Attribution();

        public Task<byte[]?> GetTileAsync(TileInfo tileInfo)
        {
            return Task.FromResult((byte[]?)_dictionary[tileInfo.Index]);
        }

        public static ITileSchema GetTileSchema()
        {
            var schema = new GlobalSphericalMercator(YAxis.TMS);
            schema.Resolutions.Clear();
            schema.Resolutions[0] = new Resolution(0, 156543.033900000);
            schema.Resolutions[1] = new Resolution(1, 78271.516950000);
            return schema;
        }

        public void AddTiles()
        {
            AddTile(new TileIndex(0, 0, 0));
            AddTile(new TileIndex(0, 0, 1));
            AddTile(new TileIndex(0, 1, 1));
            AddTile(new TileIndex(1, 0, 1));
            AddTile(new TileIndex(1, 1, 1));
        }

        private void AddTile(TileIndex tileIndex)
        {
            using var tileStream = GetTileStream(tileIndex);
            _dictionary[tileIndex] = ReadFully(tileStream);
        }

        private static Stream GetTileStream(TileIndex index)
        {
            var path = $"Mapsui.Samples.Common.GeoData.TilesAsEmbeddedResource.{index.Level}_{index.Col}_{index.Row}.png";
            var data = typeof(SampleTileSource).GetTypeInfo().Assembly.GetManifestResourceStream(path);
            return data ?? throw new Exception($"Resource could not be found: {path}");
        }

        private static byte[] ReadFully(Stream input)
        {
            using var memoryStream = new MemoryStream();
            input.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }

}
