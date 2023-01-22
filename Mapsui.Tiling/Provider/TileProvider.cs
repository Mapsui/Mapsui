// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Paul den Dulk (Geodan) as part of SharpMap

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BruTile;
using BruTile.Cache;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Providers;
using Mapsui.Tiling.Extensions;

namespace Mapsui.Tiling.Provider;

public class TileProvider : IProvider
{
    private readonly ITileSource _source;
    private readonly MemoryCache<byte[]> _bitmaps = new(100, 200);
    private readonly List<TileIndex> _queue = new();

    public MRect? GetExtent()
    {
        return _source.Schema.Extent.ToMRect();
    }

    public string? CRS { get; set; }

    public TileProvider(ITileSource tileSource)
    {
        _source = tileSource;
    }

    public async Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        var box = fetchInfo.Extent;
        var extent = new Extent(box.Min.X, box.Min.Y, box.Max.X, box.Max.Y);
        var levelId = BruTile.Utilities.GetNearestLevel(_source.Schema.Resolutions, fetchInfo.Resolution);
        var infos = _source.Schema.GetTileInfos(extent, levelId).ToList();

        var tasks = new Dictionary<TileIndex, Task>();

        foreach (var info in infos)
        {
            if (_bitmaps.Find(info.Index) != null) continue;
            if (_queue.Contains(info.Index)) continue;
            _queue.Add(info.Index);
            tasks.Add(info.Index, Task.Run(async () => await GetTileOnThreadAsync(new object[] { _source, info, _bitmaps })));
        }

        foreach (var info in infos)
        {
            if (tasks.TryGetValue(info.Index, out var task))
                await task; // wait for task to finish before loading bitmap
            var bitmap = _bitmaps.Find(info.Index);
            if (bitmap == null) continue;
            var raster = new MRaster(bitmap, new MRect(info.Extent.MinX, info.Extent.MinY, info.Extent.MaxX, info.Extent.MaxY));
            return new[] { new RasterFeature(raster) };
        }
        return Enumerable.Empty<IFeature>();
    }

    private async Task GetTileOnThreadAsync(object parameter) // This could accept normal parameters now we use PCL Profile111
    {
        var parameters = (object[])parameter;
        if (parameters.Length != 3) throw new ArgumentException("Four parameters expected");
        var tileProvider = (ITileProvider)parameters[0];
        var tileInfo = (TileInfo)parameters[1];
        var bitmap = (MemoryCache<byte[]>)parameters[2];

        try
        {
            bitmap.Add(tileInfo.Index, await tileProvider.GetTileAsync(tileInfo));
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
            // todo: report back through callback
        }
        finally
        {
            _queue.Remove(tileInfo.Index);
        }
    }
}
