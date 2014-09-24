using System;
using BruTile;
using System.Collections.Generic;
using System.IO;

namespace Mapsui.Tests.Common
{
    public class SampleTileProvider : ITileProvider
    {
        readonly IDictionary<TileIndex, byte[]> _dictionary = new Dictionary<TileIndex, byte[]>();
        Random _random = new Random();

        public SampleTileProvider()
        {
            AddTile(new TileIndex(0, 0, "0"));
            AddTile(new TileIndex(0, 0, "1"));
            AddTile(new TileIndex(0, 1, "1"));
            AddTile(new TileIndex(1, 0, "1"));
            AddTile(new TileIndex(1, 1, "1"));
        }

        private void AddTile(TileIndex tileIndex)
        {
            _dictionary[tileIndex] = ReadFully(GetTileStream(tileIndex));
        }

        public byte[] GetTile(TileInfo tileInfo)
        {
            return _dictionary[tileInfo.Index];
        }

        private static Stream GetTileStream(TileIndex tileIndex)
        {
            var path = string.Format(@"Mapsui.Tests.Common.Resources.SampleTiles.{0}_{1}_{2}.png", tileIndex.Level, tileIndex.Col, tileIndex.Row);
            var data = typeof(Utilities).Assembly.GetManifestResourceStream(path);
            if (data == null) throw new Exception("Resource could not be found: " + path);
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
}
