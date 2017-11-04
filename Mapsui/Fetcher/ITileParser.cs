using System.Collections.Generic;
using BruTile;
using Mapsui.Providers;

namespace Mapsui.Fetcher
{
    public interface ITileParser
    {
        IEnumerable<Feature> ToFeatures(TileInfo tileInfo, byte[] tileData);
    }
}
