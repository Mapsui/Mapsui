// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP023:Don't use reference types in finalizer context.", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.UI.Wpf.MapControl.Dispose(System.Boolean)")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007:Don't dispose injected.", Justification = "<Pending>", Scope = "member", Target = "~P:Mapsui.UI.Wpf.MapControl.Navigator")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007:Don't dispose injected.", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.UI.Wpf.MapControl.CommonDispose(System.Boolean)")]
[assembly: SuppressMessage("Usage", "VSTHRD001:Avoid legacy thread switching APIs", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.UI.Wpf.MapControl.RunOnUIThread(System.Action)")]
[assembly: SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.UI.Wpf.MapControl.RunOnUIThread(System.Action)")]
[assembly: SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.UI.Timer.#ctor(Mapsui.UI.TimerCallback,System.Object,System.Int32,System.Int32)")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP004:Don't ignore created IDisposable", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.UI.Wpf.MapControl.OpenBrowser(System.String)")]
