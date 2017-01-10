using System;
using System.Collections.Generic;
using Mapsui.Fetcher;
using Mapsui.Providers;

namespace Mapsui.UI
{
    public interface IMapControl
    {
        Map Map { get; set; }
        
        event EventHandler<ViewChangedEventArgs> ViewChanged;
        event EventHandler<MouseInfoEventArgs> Info;
        event EventHandler ViewportInitialized;
    }
}