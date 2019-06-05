using System;
using System.ComponentModel;
using Mapsui.Geometries;

namespace Mapsui.Fetcher
{
    interface IFetchDispatcher
    {
        bool TryTake(ref Action method);
        void SetViewport(BoundingBox extent, double resolution);

        bool Busy { get; }
        
        event DataChangedEventHandler DataChanged;
        event PropertyChangedEventHandler PropertyChanged;
    }
}