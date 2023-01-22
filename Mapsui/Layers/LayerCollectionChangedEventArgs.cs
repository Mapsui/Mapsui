using System;
using System.Collections.Generic;

namespace Mapsui.Layers;

public class LayerCollectionChangedEventArgs : EventArgs
{
    public IEnumerable<ILayer>? AddedLayers { get; }
    public IEnumerable<ILayer>? RemovedLayers { get; }
    public IEnumerable<ILayer>? MovedLayers { get; }

    public LayerCollectionChangedEventArgs(IEnumerable<ILayer>? added = null, IEnumerable<ILayer>? removed = null, IEnumerable<ILayer>? moved = null)
    {
        AddedLayers = added;
        RemovedLayers = removed;
        MovedLayers = moved;
    }
}
