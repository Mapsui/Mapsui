// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Mapsui.Layers;

namespace Mapsui.Tiling.Fetcher;

public enum TileFetchStatus
{
    /// <summary>The tile source confirmed this tile does not exist (e.g. absent in a sparse MBTiles file).</summary>
    PermanentlyUnavailable,
    /// <summary>Fetching failed too many times; no further attempts will be made.</summary>
    GaveUp,
}

/// <summary>
/// A non-renderable sentinel stored in the tile cache to record the definitive status of a tile
/// that cannot be displayed. Prevents the fetcher from retrying, and signals render strategies
/// to fall back to a lower-resolution tile instead of rendering a blank.
/// 
/// We use an IFeature as the sentinel to avoid a breaking change to the cache type parameter
/// (ITileCache&lt;IFeature?&gt;). A cleaner design would be a typed wrapper around the cache entry
/// that carries a status field directly, decoupling tile status from the feature model.
/// </summary>
internal sealed class TileFetchStatusFeature(TileFetchStatus status) : BaseFeature, IFeature
{
    public TileFetchStatus Status { get; } = status;
    public override MRect? Extent => null;
    public override void CoordinateVisitor(Action<double, double, CoordinateSetter> visit) { }
    public override object Clone() => new TileFetchStatusFeature(Status);
}
