// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP008:Don't assign member with injected and created disposables.", Justification = "<Pending>", Scope = "member", Target = "~P:Mapsui.Layers.RasterFeature.Raster")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007:Don't dispose injected.", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Layers.RasterizingLayer.DisposeRenderedGeometries(System.Collections.Generic.IEnumerable{Mapsui.IFeature})")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007:Don't dispose injected.", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Layers.RasterizingLayer.Dispose")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007:Don't dispose injected.", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.MRaster.Dispose(System.Boolean)")]
[assembly: SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Fetcher.FetchWorker.Start")]
[assembly: SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Layers.AnimatedPointLayer.UpdateData")]
[assembly: SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Layers.ImageLayer.OnPropertyChanged(System.Object,System.ComponentModel.PropertyChangedEventArgs)")]
[assembly: SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Layers.ImageLayer.StartNewFetch(Mapsui.Layers.FetchInfo)")]
[assembly: SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Rendering.VisibleFeatureIterator.IterateLayers(Mapsui.IReadOnlyViewport,System.Collections.Generic.IEnumerable{Mapsui.Layers.ILayer},System.Action{Mapsui.IReadOnlyViewport,Mapsui.Layers.ILayer,Mapsui.Styles.IStyle,Mapsui.IFeature,System.Single})")]
[assembly: SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Fetcher.FeatureFetchDispatcher`1.FetchOnThread(Mapsui.Layers.FetchInfo)")]
