using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mapsui.Rendering.OpenTK
{
    public class MapRenderer : IRenderer
    {
        public void Render(IViewport viewport, IEnumerable<ILayer> layers)
        {
            layers = layers.ToList();
            VisibleFeatureIterator.IterateLayers(viewport, layers, RenderFeature);
        }

        private static void RenderFeature(IViewport viewport, IStyle style, IFeature feature)
        {
            if (feature.Geometry is Point) 
            {
                PointRenderer.Draw(viewport, style, feature);
            }
            else if (feature.Geometry is LineString) 
            {
                LineStringRenderer.Draw(viewport, style, feature);
            }
            else if (feature.Geometry is IRaster) 
            {
                RasterRenderer.Draw(viewport, style, feature);
            }
        }

        public MemoryStream RenderToBitmapStream(IViewport viewport, IEnumerable<ILayer> layers)
        {
            throw new NotImplementedException();
        }
    }
}
