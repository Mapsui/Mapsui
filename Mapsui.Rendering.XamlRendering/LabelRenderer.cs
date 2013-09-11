using System;
using System.Linq;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
#if !NETFX_CORE
using System.Windows.Controls;
#else
using Windows.UI.Xaml.Controls;
#endif

namespace Mapsui.Rendering.XamlRendering
{
    public static class LabelRenderer
    {
        public static Canvas RenderLabelLayer(IViewport viewport, LabelLayer layer)
        {
            var canvas = new Canvas { Opacity = layer.Opacity };

            // todo: take into account the priority 
            var features = layer.GetFeaturesInView(viewport.Extent, viewport.Resolution).ToList();
            
            foreach (var layerStyle in layer.Styles)
            {
                var style = layerStyle;

                foreach (var feature in features)
                {
                    if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);

                    if ((style == null) || 
                        (style.Enabled == false) || 
                        (style.MinVisible > viewport.Resolution) || 
                        (style.MaxVisible < viewport.Resolution)) continue;

                    if (!(style is LabelStyle)) throw new Exception("Style of label is not a LabelStyle");
                    var labelStyle = style as LabelStyle;

                    labelStyle.Text = layer.GetLabelText(feature);

                    var postion = feature.Geometry.GetBoundingBox().GetCentroid();
                    canvas.Children.Add(SingleLabelRenderer.RenderLabel(postion, labelStyle, viewport));
                }
            }
            return canvas;
        }
    }
}
