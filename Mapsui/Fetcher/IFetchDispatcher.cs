using System;
using System.ComponentModel;
using Mapsui.Geometries;

namespace Mapsui.Fetcher
{
    interface IFetchDispatcher
    {
        bool TryTake(ref Action method);
        void SetViewport(BoundingBox newExtent, double newResolution);

        bool Busy { get; }
        int NumberTilesNeeded { get; }

        event DataChangedEventHandler DataChanged;
        event PropertyChangedEventHandler PropertyChanged;
    }
}