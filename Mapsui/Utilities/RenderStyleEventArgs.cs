using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mapsui.Utilities
{
    public class RenderStyleEventArgs
    {
        public RenderStyleEventArgs(object canvas, Layer layer, IFeature feature, IStyle style, float rotation)
        {
            Canvas = canvas;
            Layer = layer;
            Feature = feature;
            Style = style;
            Rotation = rotation;
        }

        public object Canvas { get; }

        public Layer Layer { get; }

        public IFeature Feature { get; }

        public IStyle Style { get; }

        public float Rotation { get; }

        public object Result { get; set; }
    }
}
