using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Nts.Widgets;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Rendering.Skia.SkiaWidgets;
using Mapsui.Styles;
using Mapsui.Widgets;
using Mapsui.Widgets.BoxWidget;
using Mapsui.Widgets.ButtonWidget;
using Mapsui.Widgets.MouseCoordinatesWidget;
using Mapsui.Widgets.LoggingWidget;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.Zoom;
using SkiaSharp;

#pragma warning disable IDISP008 // Don't assign member with injected created disposable

namespace Mapsui.Rendering.Skia;

public sealed class MapRenderer : IRenderer, IDisposable
{
    private readonly IRenderCache _renderCache;
    private long _currentIteration;
    private readonly bool _ownsRenderCache;

    public IRenderCache RenderCache => _renderCache;

    public IDictionary<Type, IWidgetRenderer> WidgetRenders { get; } = new Dictionary<Type, IWidgetRenderer>();

    /// <summary>
    /// Dictionary holding all special renderers for styles
    /// </summary>
    public IDictionary<Type, IStyleRenderer> StyleRenderers { get; } = new Dictionary<Type, IStyleRenderer>();

    static MapRenderer()
    {
        DefaultRendererFactory.Create = () => new MapRenderer();
        DefaultRendererFactory.CreateWithCache = f => new MapRenderer(f);
    }

    public MapRenderer(IRenderCache renderer)
    {
        _renderCache = renderer;
        StyleRenderers[typeof(RasterStyle)] = new RasterStyleRenderer();
        StyleRenderers[typeof(VectorStyle)] = new VectorStyleRenderer();
        StyleRenderers[typeof(LabelStyle)] = new LabelStyleRenderer();
        StyleRenderers[typeof(SymbolStyle)] = new SymbolStyleRenderer();
        StyleRenderers[typeof(CalloutStyle)] = new CalloutStyleRenderer();

        WidgetRenders[typeof(TextBox)] = new TextBoxWidgetRenderer();
        WidgetRenders[typeof(Hyperlink)] = new HyperlinkWidgetRenderer();
        WidgetRenders[typeof(ScaleBarWidget)] = new ScaleBarWidgetRenderer();
        WidgetRenders[typeof(ZoomInOutWidget)] = new ZoomInOutWidgetRenderer();
        WidgetRenders[typeof(ButtonWidget)] = new ButtonWidgetRenderer();
        WidgetRenders[typeof(BoxWidget)] = new BoxWidgetRenderer();
        WidgetRenders[typeof(MouseCoordinatesWidget)] = new MouseCoordinatesWidgetRenderer();
        WidgetRenders[typeof(EditingWidget)] = new EditingWidgetRenderer();
        WidgetRenders[typeof(MapInfoWidget)] = new MapInfoWidgetRenderer();
        WidgetRenders[typeof(LoggingWidget)] = new LoggingWidgetRenderer();
    }

    public MapRenderer() : this(new RenderCache())
    {
        _ownsRenderCache = true;
    }

    public void Render(object target, Viewport viewport, IEnumerable<ILayer> layers,
        IEnumerable<IWidget> widgets, Color? background = null)
    {
        var attributions = layers.Where(l => l.Enabled).Select(l => l.Attribution).Where(w => w != null).ToList();

        var allWidgets = widgets.Concat(attributions);

        RenderTypeSave((SKCanvas)target, viewport, layers, allWidgets, background);
    }

    private void RenderTypeSave(SKCanvas canvas, Viewport viewport, IEnumerable<ILayer> layers,
        IEnumerable<IWidget> widgets, Color? background = null)
    {
        if (!viewport.HasSize()) return;

        if (background is not null) canvas.Clear(background.ToSkia());
        Render(canvas, viewport, layers);
        Render(canvas, viewport, widgets, 1);
    }

    public MemoryStream RenderToBitmapStream(Viewport viewport, IEnumerable<ILayer> layers,
        Color? background = null, float pixelDensity = 1, IEnumerable<IWidget>? widgets = null, RenderFormat renderFormat = RenderFormat.Png)
    {
        try
        {
            var width = viewport.Width;
            var height = viewport.Height;

            var imageInfo = new SKImageInfo((int)Math.Round(width * pixelDensity), (int)Math.Round(height * pixelDensity),
                SKImageInfo.PlatformColorType, SKAlphaType.Unpremul);

            MemoryStream memoryStream = new MemoryStream();

            switch (renderFormat)
            {
                case RenderFormat.Skp:
                    {
                        using var pictureRecorder = new SKPictureRecorder();
                        using var skCanvas = pictureRecorder.BeginRecording(new SKRect(0, 0, Convert.ToSingle(width), Convert.ToSingle(height)));
                        RenderTo(viewport, layers, background, pixelDensity, widgets, skCanvas);
                        using var skPicture = pictureRecorder.EndRecording();
                        skPicture?.Serialize(memoryStream);
                        break;
                    }
                case RenderFormat.Png:
                    {
                        using var surface = SKSurface.Create(imageInfo);
                        using var skCanvas = surface.Canvas;
                        RenderTo(viewport, layers, background, pixelDensity, widgets, skCanvas);
                        using var image = surface.Snapshot();
                        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                        data.SaveTo(memoryStream);
                        break;
                    }
                case RenderFormat.WebP:
                    {
                        using var surface = SKSurface.Create(imageInfo);
                        using var skCanvas = surface.Canvas;
                        RenderTo(viewport, layers, background, pixelDensity, widgets, skCanvas);
                        using var image = surface.Snapshot();
                        var options = new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossless, 100);
                        using var peekPixels = image.PeekPixels();
                        using var data = peekPixels.Encode(options);
                        data.SaveTo(memoryStream);
                        break;
                    }
            }

            return memoryStream;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message);
            throw;
        }
    }

    private void RenderTo(Viewport viewport, IEnumerable<ILayer> layers, Color? background, float pixelDensity,
        IEnumerable<IWidget>? widgets, SKCanvas skCanvas)
    {
        if (skCanvas == null) throw new ArgumentNullException(nameof(viewport));

        // Not sure if this is needed here:
        if (background is not null) skCanvas.Clear(background.ToSkia());
        skCanvas.Scale(pixelDensity, pixelDensity);
        Render(skCanvas, viewport, layers);
        if (widgets is not null)
            Render(skCanvas, viewport, widgets, 1);
    }

    private void Render(SKCanvas canvas, Viewport viewport, IEnumerable<ILayer> layers)
    {
        try
        {
            layers = layers.ToList();

            VisibleFeatureIterator.IterateLayers(viewport, layers, _currentIteration, (v, l, s, f, o, i) => { RenderFeature(canvas, v, l, s, f, o, i); });

            _currentIteration++;
        }
        catch (Exception exception)
        {
            Logger.Log(LogLevel.Error, "Unexpected error in skia renderer", exception);
        }
    }

    private void RenderFeature(SKCanvas canvas, Viewport viewport, ILayer layer, IStyle style, IFeature feature, float layerOpacity, long iteration)
    {
        // Check, if we have a special renderer for this style
        if (StyleRenderers.TryGetValue(style.GetType(), out var renderer))
        {
            // Save canvas
            canvas.Save();
            // We have a special renderer, so try, if it could draw this
            var styleRenderer = (ISkiaStyleRenderer)renderer;
            var result = styleRenderer.Draw(canvas, viewport, layer, feature, style, _renderCache, iteration);
            // Restore old canvas
            canvas.Restore();
            // Was it drawn?
            if (result)
                // Yes, special style renderer drawn correct
                return;
        }
    }

    private void Render(object canvas, Viewport viewport, IEnumerable<IWidget> widgets, float layerOpacity)
    {
        WidgetRenderer.Render(canvas, viewport, widgets, WidgetRenders, layerOpacity);
    }

    public MapInfo? GetMapInfo(double x, double y, Viewport viewport, IEnumerable<ILayer> layers, int margin = 0)
    {
        // todo: use margin to increase the pixel area
        // todo: We will need to select on style instead of layer

        var mapInfoLayers = layers
            .Select(l => l is ISourceLayer sl and not ILayerFeatureInfo ? sl.SourceLayer : l)
            .Where(l => l.IsMapInfoLayer)
            .ToList();

        var tasks = new List<Task>();

        var list = new List<MapInfoRecord>[mapInfoLayers.Count];
        var result = new MapInfo(new MPoint(x, y), viewport.ScreenToWorld(x, y), viewport.Resolution);

        if (!viewport.ToExtent()?.Contains(viewport.ScreenToWorld(result.ScreenPosition)) ?? false) return result;

        try
        {
            var width = (int)viewport.Width;
            var height = (int)viewport.Height;

            var imageInfo = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Unpremul);

            var intX = (int)x;
            var intY = (int)y;

            if (intX >= width || intY >= height)
                return result;

            using (var surface = SKSurface.Create(imageInfo))
            {
                if (surface == null) return null;

                surface.Canvas.ClipRect(new SKRect((float)(x - 1), (float)(y - 1), (float)(x + 1), (float)(y + 1)));
                surface.Canvas.Clear(SKColors.Transparent);

                using var pixMap = surface.PeekPixels();
                var color = pixMap.GetPixelColor(intX, intY);

                for (var index = 0; index < mapInfoLayers.Count; index++)
                {
                    var currentIndex = mapInfoLayers.Count - index - 1; // for having copy of index for thread safe access and reverse order.
                    var mapList = list[currentIndex] = new List<MapInfoRecord>();
                    var infoLayer = mapInfoLayers[index];
                    if (infoLayer is ILayerFeatureInfo layerFeatureInfo)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                // creating new list to avoid multithreading problems
                                mapList = new List<MapInfoRecord>();
                                // get information from ILayer Feature Info
                                var features = await layerFeatureInfo.GetFeatureInfoAsync(viewport, x, y);
                                foreach (var it in features)
                                {
                                    foreach (var feature in it.Value)
                                    {
                                        mapList.Add(new MapInfoRecord(feature, infoLayer.Style!, infoLayer));
                                    }
                                }

                                // atomic replace of new list is thread safe.a
                                list[currentIndex] = mapList;
                            }
                            catch (Exception e)
                            {
                                Logger.Log(LogLevel.Error, e.Message, e);
                            }
                        }));
                    }
                    else
                    {
                        // get information from ILayer
                        VisibleFeatureIterator.IterateLayers(viewport, new[] { infoLayer }, 0,
                            (v, layer, style, feature, opacity, iteration) =>
                            {
                                try
                                {
                                    // ReSharper disable AccessToDisposedClosure // There is no delayed fetch. After IterateLayers returns all is done. I do not see a problem.
                                    surface.Canvas.Save();
                                    // 1) Clear the entire bitmap
                                    surface.Canvas.Clear(SKColors.Transparent);
                                    // 2) Render the feature to the clean canvas
                                    RenderFeature(surface.Canvas, v, layer, style, feature, opacity, 0);
                                    // 3) Check if the pixel has changed.
                                    if (color != pixMap.GetPixelColor(intX, intY))
                                        // 4) Add feature and style to result
                                        mapList.Add(new MapInfoRecord(feature, style, layer));
                                    surface.Canvas.Restore();
                                    // ReSharper restore AccessToDisposedClosure
                                }
                                catch (Exception exception)
                                {
                                    Logger.Log(LogLevel.Error,
                                        "Unexpected error in the code detecting if a feature is clicked. This uses SkiaSharp.",
                                        exception);
                                }
                            });
                    }
                }
            }

            var mapInfos = list.SelectMany(f => f);
            var task = Task.WhenAll(tasks);
            result = new MapInfo(result, mapInfos, task);
        }
        catch (Exception exception)
        {
            Logger.Log(LogLevel.Error, "Unexpected error in skia renderer", exception);
        }

        return result;
    }

    public void Dispose()
    {
        if (_ownsRenderCache)
        {
            _renderCache.Dispose();    
        }
    }
}
