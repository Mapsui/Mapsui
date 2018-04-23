using System;
using System.ComponentModel;
using Mapsui.Geometries;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Mapsui.Tests")]
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