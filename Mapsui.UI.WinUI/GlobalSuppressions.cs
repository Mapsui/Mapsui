// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007:Don't dispose injected.", Justification = "<Pending>", Scope = "member", Target = "~P:Mapsui.UI.WinUI.MapControl.Navigator")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007:Don't dispose injected.", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.UI.WinUI.MapControl.CommonDispose(System.Boolean)")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP023:Don't use reference types in finalizer context.", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.UI.WinUI.MapControl.Dispose(System.Boolean)")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP008:Don't assign member with injected and created disposables.", Justification = "<Pending>", Scope = "member", Target = "~F:Mapsui.UI.WinUI.MapControl._map")]
[assembly: SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.UI.WinUI.MapControl.OpenBrowser(System.String)")]
[assembly: SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.UI.WinUI.MapControl.RunOnUIThread(System.Action)")]
[assembly: SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "<Pending>", Scope = "member", Target = "~M:Mapsui.UI.Timer.#ctor(Mapsui.UI.TimerCallback,System.Object,System.Int32,System.Int32)")]
