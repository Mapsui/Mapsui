using System;
using Mapsui.Layers;
using System.Windows.Controls;

namespace Mapsui.Rendering.Xaml
{
    [Obsolete("Use StackedLabelProvider instead", true)]
    public static class StackedLabelLayerRenderer
    {
        public static Canvas Render(IViewport viewport, LabelLayer layer)
        {
            throw new NotImplementedException();
        }
    }
}