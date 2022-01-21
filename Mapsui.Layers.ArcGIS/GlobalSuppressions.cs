// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007:Don't dispose injected.", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Providers.ArcGIS.CapabilitiesHelper.CopyAndClose(System.IO.Stream)~System.IO.Stream")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007:Don't dispose injected.", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Providers.ArcGIS.Dynamic.ArcGISIdentify.CopyAndClose(System.IO.Stream)~System.IO.Stream")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created.", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Providers.ArcGIS.Dynamic.ArcGISDynamicProvider.GetFeatures(Mapsui.Layers.FetchInfo)~System.Collections.Generic.IEnumerable{Mapsui.IFeature}")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created.", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Providers.ArcGIS.Image.ArcGISImageServiceProvider.GetFeatures(Mapsui.Layers.FetchInfo)~System.Collections.Generic.IEnumerable{Mapsui.IFeature}")]
[assembly: SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Providers.ArcGIS.CapabilitiesHelper.ExecuteRequest(System.String,Mapsui.Providers.ArcGIS.CapabilitiesType,System.Net.ICredentials,System.String)")]
[assembly: SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Providers.ArcGIS.Dynamic.ArcGISIdentify.Request(System.String,System.Double,System.Double,System.Int32,System.String[],System.Double,System.Double,System.Double,System.Double,System.Double,System.Double,System.Double,System.Boolean,System.Net.ICredentials,System.Int32)")]
[assembly: SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Providers.ArcGIS.Dynamic.ArcGISDynamicProvider.TryGetMap(Mapsui.IViewport,Mapsui.MRaster@)~System.Boolean")]
[assembly: SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.Providers.ArcGIS.Image.ArcGISImageServiceProvider.TryGetMap(Mapsui.IViewport,Mapsui.MRaster@)~System.Boolean")]
