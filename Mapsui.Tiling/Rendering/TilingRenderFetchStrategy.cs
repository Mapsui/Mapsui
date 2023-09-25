using System.Collections.Generic;
using System.Linq;
using BruTile;
using BruTile.Cache;
using Mapsui.Tiling.Extensions;

namespace Mapsui.Tiling.Rendering;

public class TilingRenderFetchStrategy : IRenderFetchStrategy
{
    private readonly IRenderFetchStrategy _renderFetchStrategy;
    private IList<IFeature>? _lastFetch;

    public TilingRenderFetchStrategy(IRenderFetchStrategy? renderFetchStrategy)
    {
        _renderFetchStrategy = renderFetchStrategy ?? new RenderFetchStrategy();
    }

    public IList<IFeature> Get(MRect extent, double resolution, ITileSchema schema, ITileCache<IFeature?> memoryCache)
    {
        var result = _renderFetchStrategy.Get(extent, resolution, schema, memoryCache);
        FillMissingFeatures(result, extent, resolution, schema);
        _lastFetch = result;
        return result;
    }

    private void FillMissingFeatures(IList<IFeature> result, MRect extent, double resolution, ITileSchema schema)
    {
        var tiles = schema.GetTileInfos(extent.ToExtent(), resolution);
        var missingFeatures = new List<IFeature>();
        foreach (var tileInfo in tiles)
        {
            var mRect = tileInfo.Extent.ToMRect();
            if (!result.Any(f => mRect.Equals(f.Extent)))
            {
                var missingTile = _lastFetch?.FirstOrDefault(f =>  mRect.Equals(f.Extent));
                if (missingTile != null)
                {
                    missingFeatures.Add(missingTile);
                }
            }
        }

        foreach (var tile in missingFeatures)
        {
            result.Add(tile);
        }
    }
}
