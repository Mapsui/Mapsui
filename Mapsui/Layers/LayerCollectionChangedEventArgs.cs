using System;
using System.Collections.Generic;

namespace Mapsui.Layers;

public class LayerCollectionChangedEventArgs(IEnumerable<ILayer> added, IEnumerable<ILayer> removed, IEnumerable<ILayer> moved) : EventArgs
{
    public IEnumerable<ILayer> AddedLayers { get; } = added;
    public IEnumerable<ILayer> RemovedLayers { get; } = removed;
    public IEnumerable<ILayer> MovedLayers { get; } = moved;
}
