using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;
using Mapsui.Benchmark;
using Mapsui.Widgets;

namespace Mapsui.Rendering.Xaml.XamlWidgets
{
    public static class WidgetRenderer
    {
        public static void Render(Canvas canvas, IReadOnlyViewport viewport, 
            IEnumerable<IWidget> widgets, IDictionary<Type, IWidgetRenderer> renderers,List<RenderBenchmark> benchmarks)
        {
            var widgetCanvas = new Canvas
            {
                Width = canvas.ActualWidth,
                Height = canvas.ActualHeight,
                Background = null
            };

            widgetCanvas.Arrange(new System.Windows.Rect(canvas.RenderSize));

            canvas.Children.Add(widgetCanvas);

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

                ((IXamlWidgetRenderer)renderers[widget.GetType()]).Draw(widgetCanvas, viewport, widget);
                if (benchmarks != null)
                {
                    sw.Stop();
                    benchmarks[i - 1].Time = sw.Elapsed.TotalMilliseconds;
                }
            }
        }
    }
}