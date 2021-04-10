using Mapsui.Benchmark;
using Mapsui.Widgets;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mapsui.Rendering.Skia.SkiaWidgets
{
    public static class WidgetRenderer
    {
        public static void Render(SKCanvas canvas, IReadOnlyViewport viewport, IEnumerable<IWidget> widgets,
            IDictionary<Type, IWidgetRenderer> renders, float layerOpacity, List<RenderBenchmark> benchmarks)
        {
            var sw = new Stopwatch();

            int i = 0;
            foreach (var widget in widgets)
            {
                ++i;
                if (!widget.Enabled) continue;
                if (benchmarks != null)
                {
                    sw.Reset();
                    sw.Start();
                }

                ((ISkiaWidgetRenderer)renders[widget.GetType()]).Draw(canvas, viewport, widget, layerOpacity);

                if (benchmarks != null)
                {
                    sw.Stop();
                    benchmarks[i - 1].Time = sw.Elapsed.TotalMilliseconds;
                }
            }
        }
    }
}