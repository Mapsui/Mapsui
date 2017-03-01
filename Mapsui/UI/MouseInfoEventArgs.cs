using System;
using Mapsui.Providers;

namespace Mapsui.UI
{
    public class MouseInfoEventArgs : EventArgs
    {
        public string LayerName { get; set; } = "";
        public IFeature Feature { get; set; }
        public bool Leaving { get; set; }
    }
}