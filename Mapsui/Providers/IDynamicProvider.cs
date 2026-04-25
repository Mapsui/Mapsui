// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using Mapsui.Layers;

namespace Mapsui.Providers;

/// <summary>
/// A provider that actively notifies consumers when its data has changed.
/// Combines <see cref="IProvider"/> (data retrieval) with <see cref="IDynamic"/>
/// (change notification) so that layers can subscribe to updates without
/// needing to poll.
/// </summary>
public interface IDynamicProvider : IProvider, IDynamic
{
}
