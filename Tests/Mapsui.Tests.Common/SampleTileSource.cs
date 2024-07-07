using BruTile;
using BruTile.Predefined;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Mapsui.Tests.Common;

internal class SampleTileSource : ILocalTileSource
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
        var path = $"Mapsui.Tests.Common.Resources.SampleTiles.{index.Level}_{index.Col}_{index.Row}.png";
        var data = typeof(SampleTileSource).GetTypeInfo().Assembly.GetManifestResourceStream(path);
        return data ?? throw new Exception($"Resource could not be found: {path}");
    }

    public static byte[] ReadFully(Stream input)
    {
        using var memoryStream = new MemoryStream();
        input.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}
