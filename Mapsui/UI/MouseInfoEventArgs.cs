using System;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;

namespace Mapsui.UI
{
    public class MouseInfoEventArgs : EventArgs
    {
        public ILayer Layer { get; set; }
        public IFeature Feature { get; set; }
        public Point WorldPosition { get; set; }
    }
}