// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Tiling.Provider.TileProvider.FetchTiles(Mapsui.Layers.FetchInfo)~System.Collections.Generic.IEnumerable{Mapsui.IFeature}")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Tiling.Fetcher.TileFetchDispatcher.FetchOnThread(BruTile.TileInfo)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Tiling.Layers.TmsTileSourceBuilder.BuildAsync(System.String,System.Boolean,BruTile.Cache.IPersistentCache{System.Byte[]})~BruTile.ITileSource")]
