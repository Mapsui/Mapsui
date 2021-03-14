using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mapsui.Widgets;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets
{
    public static class WidgetRenderer
    {
        public static void Render(SKCanvas target, IReadOnlyViewport viewport, IEnumerable<IWidget> widgets,
            IDictionary<Type, IWidgetRenderer> renders, float layerOpacity, List<RenderBenchmark> currentBenchmarks)
        {
            var canvas = (SKCanvas) target;
            var sw = new Stopwatch();

            int i = 0;
            foreach (var widget in widgets)
            {
                if (!widget.Enabled) continue;
                sw.Reset();
                sw.Start();

                ((ISkiaWidgetRenderer)renders[widget.GetType()]).Draw(canvas, viewport, widget, layerOpacity);

                sw.Stop();

                if (currentBenchmarks != null)
                    currentBenchmarks[i].Time = sw.Elapsed.TotalMilliseconds;
            }
        }
    }
}