// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using BruTile;

namespace Mapsui.Tiling.Fetcher;

public interface IDataFetchStrategy
{
    /// <summary>
    /// Fetches the tiles from the data source to put them into the cache. A strategy could  pre-fetch
    /// certain tiles to anticipate future use.
    /// </summary>
    IList<TileInfo> Get(ITileSchema schema, Extent extent, int level);
}
