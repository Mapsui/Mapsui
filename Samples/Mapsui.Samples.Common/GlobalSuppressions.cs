// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// Assembly-wide suppressions — omit Target to apply to every violation of the diagnostic
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created", Justification = "Assembly-wide suppression while addressing analyzer output")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP004:Don't ignore created IDisposable", Justification = "Assembly-wide suppression while addressing analyzer output")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP003:Dispose previous before re-assigning", Justification = "Assembly-wide suppression while addressing analyzer output")]
[assembly: SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP007:Don't dispose injected", Justification = "Assembly-wide suppression while addressing analyzer output")]
