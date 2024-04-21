using BruTile;
using Mapsui.Fetcher;
using System;

namespace Mapsui.Tiling.Fetcher;
internal class TileDataChangedEventArgs(Exception? exception, TileInfo tileInfo) : DataChangedEventArgs(exception)
{
    public TileInfo TileInfo { get; } = tileInfo;
}
