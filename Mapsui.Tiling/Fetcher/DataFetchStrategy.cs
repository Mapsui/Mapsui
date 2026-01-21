// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using BruTile;
using Mapsui.Utilities;

namespace Mapsui.Tiling.Fetcher;

public class DataFetchStrategy : IDataFetchStrategy
{
    private readonly int _maxLevelsUp;
    private readonly double _additionalMarginAsPercentage;

    public DataFetchStrategy(int maxLevelsUp = int.MaxValue, double additionalMarginAsPercentage = 0)
    {
        if (additionalMarginAsPercentage < 0 || additionalMarginAsPercentage > 100)
            throw new ArgumentOutOfRangeException(nameof(additionalMarginAsPercentage), additionalMarginAsPercentage, "Value must be between 0 and 100.");

        _maxLevelsUp = maxLevelsUp;
        _additionalMarginAsPercentage = additionalMarginAsPercentage;
    }

    public IList<TileInfo> Get(ITileSchema schema, Extent extent, int level)
    {
        var expandedExtent = ExpandExtent(extent, _additionalMarginAsPercentage);

        var tileInfos = new List<TileInfo>();
        // Iterating through all levels from current to zero. If lower levels are
        // not available the renderer can fall back on higher level tiles. 
        var resolution = schema.Resolutions[level].UnitsPerPixel;
        var levels = schema.Resolutions.Where(k => k.Value.UnitsPerPixel >= resolution).OrderBy(x => x.Value.UnitsPerPixel).ToList();

        var counter = 0;
        foreach (var l in levels)
        {
            if (counter > _maxLevelsUp) break;

            var tileInfosForLevel = schema.GetTileInfos(expandedExtent, l.Key).OrderBy(
                t => Algorithms.Distance(expandedExtent.CenterX, expandedExtent.CenterY, t.Extent.CenterX, t.Extent.CenterY));

            tileInfos.AddRange(tileInfosForLevel);
            counter++;
        }

        return tileInfos;
    }

    private static Extent ExpandExtent(Extent extent, double marginPercentage)
    {
        if (marginPercentage == 0)
            return extent;

        var marginFraction = marginPercentage / 100.0;
        var marginX = extent.Width * marginFraction;
        var marginY = extent.Height * marginFraction;

        return new Extent(
            extent.MinX - marginX,
            extent.MinY - marginY,
            extent.MaxX + marginX,
            extent.MaxY + marginY);
    }
}
