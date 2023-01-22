using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using BruTile;

namespace Mapsui.Tests.Common;

public class SampleTileProvider : ITileProvider
{
    private readonly IDictionary<TileIndex, byte[]> _dictionary = new Dictionary<TileIndex, byte[]>();

    public SampleTileProvider()
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

    public Task<byte[]> GetTileAsync(TileInfo tileInfo)
    {
        return Task.FromResult(_dictionary[tileInfo.Index]);
    }

    private static Stream GetTileStream(TileIndex index)
    {
        var path = $"Mapsui.Tests.Common.Resources.SampleTiles.{index.Level}_{index.Col}_{index.Row}.png";
        var data = typeof(Utilities).GetTypeInfo().Assembly.GetManifestResourceStream(path);
        if (data == null) throw new Exception($"Resource could not be found: {path}");
        return data;
    }

    public static byte[] ReadFully(Stream input)
    {
        using (var ms = new MemoryStream())
        {
            input.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
