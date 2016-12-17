using System;
using Mapsui.Layers;
#if !NETFX_CORE
using System.Windows.Controls;
#else
using Windows.UI.Xaml.Controls;
#endif

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