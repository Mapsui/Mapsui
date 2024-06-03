// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created.", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Providers.Wms.Client.GetStreamAsync(System.String)~System.Threading.Tasks.Task{System.IO.Stream}")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created.", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Providers.Wms.WmsProvider.GetStreamAsync(System.String)~System.Threading.Tasks.Task{System.IO.Stream}")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP008:Don't assign member with injected and created disposables.", Justification = "<Pending>", Scope = "member", Target = "~P:Mapsui.Layers.RasterFeature.Raster")]
[assembly: SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Layers.ImageLayer.StartNewFetch(Mapsui.Layers.FetchInfo)")]
[assembly: SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Extensions.Catch.Exceptions(System.Func{System.Threading.Tasks.Task})")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Providers.Wms.GetFeatureInfo.GetStreamAsync(System.String)~System.Threading.Tasks.Task{System.IO.Stream}")]
[assembly: SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.MapInfo.GetMapInfoAsync~System.Threading.Tasks.Task{Mapsui.MapInfoBase}")]
