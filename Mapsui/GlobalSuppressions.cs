// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created.", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Providers.Wms.Client.GetStreamAsync(System.String)~System.Threading.Tasks.Task{System.IO.Stream}")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created.", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Providers.Wms.GetFeatureInfo.GetStreamAsync(System.String)~System.Threading.Tasks.Task{System.IO.Stream}")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created.", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Providers.Wms.WmsProvider.GetFeatures(Mapsui.Layers.FetchInfo)~System.Collections.Generic.IEnumerable{Mapsui.IFeature}")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created.", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Providers.Wms.WmsProvider.GetStreamAsync(System.String)~System.Threading.Tasks.Task{System.IO.Stream}")]
[assembly: SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Providers.Wms.Client.GetRemoteXml(System.String)~System.Xml.XmlDocument")]
[assembly: SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Providers.Wms.GetFeatureInfo.Request(System.String,System.String,System.String,System.String,System.String,System.Double,System.Double,System.Double,System.Double,System.Int32,System.Int32,System.Int32,System.Int32)")]
[assembly: SuppressMessage("Usage", "VSTHRD104:Offer async methods", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Providers.Wms.WmsProvider.TryGetMap(Mapsui.IViewport,Mapsui.MRaster@)~System.Boolean")]
[assembly: SuppressMessage("Usage", "VSTHRD104:Offer async methods", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Providers.Wms.WmsProvider.GetLegendsAsync~System.Collections.Generic.IEnumerable{System.IO.MemoryStream}")]
[assembly: SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Providers.Wms.WmsProvider.TryGetMap(Mapsui.IViewport,Mapsui.MRaster@)~System.Boolean")]
[assembly: SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Providers.Wms.WmsProvider.GetLegendsAsync~System.Collections.Generic.IEnumerable{System.IO.MemoryStream}")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP008:Don't assign member with injected and created disposables.", Justification = "<Pending>", Scope = "member", Target = "~P:Mapsui.Layers.RasterFeature.Raster")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007:Don't dispose injected.", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Layers.RasterizingLayer.DisposeRenderedGeometries(System.Collections.Generic.IEnumerable{Mapsui.IFeature})")]
[assembly: SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Fetcher.FetchWorker.Start")]
[assembly: SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Layers.ImageLayer.OnPropertyChanged(System.Object,System.ComponentModel.PropertyChangedEventArgs)")]
[assembly: SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Layers.ImageLayer.StartNewFetch(Mapsui.Layers.FetchInfo)")]
[assembly: SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Providers.AsyncProviderBase`1.GetFeatures(Mapsui.Layers.FetchInfo)~System.Collections.Generic.IEnumerable{`0}")]
[assembly: SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Extensions.Catch.Exceptions(System.Func{System.Threading.Tasks.Task})")]
