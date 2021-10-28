using System;
using System.ComponentModel;
using Mapsui.Layers;

namespace Mapsui.Fetcher
{
    interface IFetchDispatcher
    {
        bool TryTake(ref Action method);
        void SetViewport(FetchInfo fetchInfo);
        bool Busy { get; }
        event DataChangedEventHandler?  DataChanged;
        event PropertyChangedEventHandler?  PropertyChanged;
    }
}