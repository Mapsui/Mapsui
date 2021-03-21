using System;
using System.Collections.Generic;
using System.IO;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Widgets;

namespace Mapsui.Rendering
{
    public class RenderBenchmark
    {
        public string Name { get; set; }
        public double Time { get; set; }

        public int FeatureCount { get; set; }
        public int StyleCount { get; set; }

        public Dictionary<string,int> StyleTypeCount;
        public Dictionary<string,double> StyleTypeTime;
    }

    public interface IRenderer : IRenderInfo
    {
        void Render(object target, IReadOnlyViewport viewport, IEnumerable<ILayer> layers, IEnumerable<IWidget> widgets, Color background = null);
        MemoryStream RenderToBitmapStream(IReadOnlyViewport viewport, IEnumerable<ILayer> layers, Color background = null, float pixelDensity = 1);
        ISymbolCache SymbolCache { get; }
        IDictionary<Type, IWidgetRenderer> WidgetRenders { get; }
        IDictionary<Type, IStyleRenderer> StyleRenderers { get; }

        List<List<RenderBenchmark>> Benchmarks { get; }
    }
}
