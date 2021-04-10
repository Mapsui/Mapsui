using Mapsui.Benchmark;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mapsui.Rendering
{
    public static class VisibleFeatureIterator
    {
        public static void IterateLayers(IReadOnlyViewport viewport, IEnumerable<ILayer> layers,
            Action<IReadOnlyViewport, ILayer, IStyle, IFeature, float> callback,
            List<RenderBenchmark> benchmarks
            )
        {
            var sw = new Stopwatch();

            int i = 0;
            foreach (var layer in layers)
            {
                ++i;
                if (layer.Enabled == false) continue;
                if (layer.MinVisible > viewport.Resolution) continue;
                if (layer.MaxVisible < viewport.Resolution) continue;

                RenderBenchmark currentBenchmark = null;
                if (benchmarks != null)
                {
                    sw.Reset();
                    sw.Start();
                    currentBenchmark = benchmarks[i-1];
                }

                IterateLayer(viewport, layer, callback, currentBenchmark);
                if (currentBenchmark != null)
                {
                    sw.Stop();
                    currentBenchmark.Time = sw.Elapsed.TotalMilliseconds;
                }
            }
        }

        private static void IterateLayer(IReadOnlyViewport viewport, ILayer layer,
            Action<IReadOnlyViewport, ILayer, IStyle, IFeature, float> callback,
            RenderBenchmark currentBenchmark)
        {
            var features = layer.GetFeaturesInView(viewport.Extent, viewport.Resolution).ToList();

            var layerStyles = ToArray(layer);
            int stylesCount = 0;
            int featureCount = 0;

            foreach (var layerStyle in layerStyles)
            {
                var style = layerStyle; // This is the default that could be overridden by an IThemeStyle

                foreach (var feature in features)
                {
                    if (layerStyle is IThemeStyle) style = (layerStyle as IThemeStyle).GetStyle(feature);
                    if (ShouldNotBeApplied(style, viewport)) continue;

                    if (style is StyleCollection styles) // The ThemeStyle can again return a StyleCollection
                    {
                        foreach (var s in styles)
                        {
                            if (ShouldNotBeApplied(s, viewport)) continue;
                            callback(viewport, layer, s, feature, (float)layer.Opacity);
                            ++stylesCount;
                        }
                    }
                    else
                    {
                        callback(viewport, layer, style, feature, (float)layer.Opacity);
                        ++stylesCount;
                    }
                    ++featureCount;
                }
            }

            foreach (var feature in features)
            {
                var featureStyles = feature.Styles ?? Enumerable.Empty<IStyle>(); // null check
                foreach (var featureStyle in featureStyles)
                {
                    if (ShouldNotBeApplied(featureStyle, viewport)) continue;

                    callback(viewport, layer, featureStyle, feature, (float)layer.Opacity);
                    ++stylesCount;
                }
                ++featureCount;
            }

            if (currentBenchmark !=null)
            {
                currentBenchmark.StyleCount = stylesCount;
                currentBenchmark.FeatureCount = featureCount;
            }
        }

        private static bool ShouldNotBeApplied(IStyle style, IReadOnlyViewport viewport)
        {
            return style == null || !style.Enabled || style.MinVisible > viewport.Resolution || style.MaxVisible < viewport.Resolution;
        }

        private static IStyle[] ToArray(ILayer layer)
        {
            return (layer.Style as StyleCollection)?.ToArray() ?? new[] { layer.Style };
        }
    }
}