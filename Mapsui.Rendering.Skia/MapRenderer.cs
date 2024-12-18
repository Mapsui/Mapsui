using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Manipulations;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Rendering.Skia.SkiaWidgets;
using Mapsui.Styles;
using Mapsui.Widgets;
using Mapsui.Widgets.BoxWidgets;
using Mapsui.Widgets.ButtonWidgets;
using Mapsui.Widgets.InfoWidgets;
using Mapsui.Widgets.ScaleBar;
using SkiaSharp;

namespace Mapsui.Rendering.Skia;

public sealed class MapRenderer : IRenderer, IDisposable
{
    private readonly RenderService _renderService;
    private long _currentIteration;

    static MapRenderer()
    {
        DefaultRendererFactory.Create = () => new MapRenderer();
    }

    public bool EnabledVectorCache
    {
        get => _renderService.VectorCache.Enabled;
        set => _renderService.VectorCache.Enabled = value;
    }

    public IRenderService RenderService => _renderService;
    public IDictionary<Type, IWidgetRenderer> WidgetRenders { get; } = new Dictionary<Type, IWidgetRenderer>();
    /// <summary>
    /// Dictionary holding all special renderers for styles
    /// </summary>
    public IDictionary<Type, IStyleRenderer> StyleRenderers { get; } = new Dictionary<Type, IStyleRenderer>();

    public ImageSourceCache ImageSourceCache => _renderService.ImageSourceCache;

    private void InitRenderer()
    {
        StyleRenderers[typeof(RasterStyle)] = new RasterStyleRenderer();
        StyleRenderers[typeof(VectorStyle)] = new VectorStyleRenderer();
        StyleRenderers[typeof(LabelStyle)] = new LabelStyleRenderer();
        StyleRenderers[typeof(SymbolStyle)] = new SymbolStyleRenderer();
        StyleRenderers[typeof(CalloutStyle)] = new CalloutStyleRenderer();

        WidgetRenders[typeof(TextBoxWidget)] = new TextBoxWidgetRenderer();
        WidgetRenders[typeof(ScaleBarWidget)] = new ScaleBarWidgetRenderer();
        WidgetRenders[typeof(ZoomInOutWidget)] = new ZoomInOutWidgetRenderer();
        WidgetRenders[typeof(ImageButtonWidget)] = new ImageButtonWidgetRenderer();
        WidgetRenders[typeof(BoxWidget)] = new BoxWidgetRenderer();
        WidgetRenders[typeof(LoggingWidget)] = new LoggingWidgetRenderer();
        WidgetRenders[typeof(InputOnlyWidget)] = new InputOnlyWidgetRenderer();
    }

    public MapRenderer() : this(10000)
    { }

    public MapRenderer(int vectorCacheCapacity)
    {
        // Todo: Think about an alternative to initialize. Perhaps the capacity should
        // be determined by the number of features used in one Paint iteration.
        _renderService = new RenderService(vectorCacheCapacity);
        InitRenderer();
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
        Color? background = null, float pixelDensity = 1, IEnumerable<IWidget>? widgets = null, RenderFormat renderFormat = RenderFormat.Png, int quality = 100)
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
                        using var skCanvas = pictureRecorder.BeginRecording(new SKRect(0, 0, (float)width, (float)height));
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
                        var options = new SKPngEncoderOptions(SKPngEncoderFilterFlags.AllFilters, 9); // 9 is the highest compression
                        using var peekPixels = image.PeekPixels();
                        using var data = peekPixels.Encode(options) ?? throw new NotSupportedException();
                        data.SaveTo(memoryStream);
                        break;
                    }
                case RenderFormat.WebP:
                    {
                        using var surface = SKSurface.Create(imageInfo);
                        using var skCanvas = surface.Canvas;
                        RenderTo(viewport, layers, background, pixelDensity, widgets, skCanvas);
                        using var image = surface.Snapshot();
                        var compression = quality == 100
                            ? SKWebpEncoderCompression.Lossless
                            : SKWebpEncoderCompression.Lossy;
                        var options = new SKWebpEncoderOptions(compression, quality);
                        using var peekPixels = image.PeekPixels();
                        using var data = peekPixels.Encode(options) ?? throw new NotSupportedException();
                        data.SaveTo(memoryStream);
                        break;
                    }
                case RenderFormat.Jpeg:
                    {
                        using var surface = SKSurface.Create(imageInfo);
                        using var skCanvas = surface.Canvas;
                        skCanvas.Clear(SKColors.White); // Avoiding Black Background when Transparent Pixels
                        RenderTo(viewport, layers, background, pixelDensity, widgets, skCanvas);
                        using var image = surface.Snapshot();
                        var options = new SKJpegEncoderOptions(quality, SKJpegEncoderDownsample.Downsample420, SKJpegEncoderAlphaOption.Ignore);
                        using var peekPixels = image.PeekPixels();
                        using var data = peekPixels.Encode(options) ?? throw new NotSupportedException();
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

            VisibleFeatureIterator.IterateLayers(viewport, layers, _currentIteration, (v, l, s, f, o, i) => RenderFeature(canvas, v, l, s, f, o, i));

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
            var result = styleRenderer.Draw(canvas, viewport, layer, feature, style, _renderService, iteration);
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
        WidgetRenderer.Render(canvas, viewport, widgets, WidgetRenders, _renderService, layerOpacity);
    }

    public MapInfo GetMapInfo(double x, double y, Viewport viewport, IEnumerable<ILayer> layers, int margin = 0)
    {
        // Todo: Use margin to increase the pixel area
        // Todo: Select on style instead of layer

        var mapInfoLayers = layers
            .Select(l => l is ISourceLayer sl and not ILayerFeatureInfo ? sl.SourceLayer : l)
            .Where(l => l.IsMapInfoLayer)
            .ToList();

        var tasks = new List<Task>();

        var list = new List<MapInfoRecord>[mapInfoLayers.Count];
        var result = new MapInfo(new ScreenPosition(x, y), viewport.ScreenToWorld(x, y), viewport.Resolution);

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
                if (surface == null)
                {
                    Logger.Log(LogLevel.Error, "SKSurface is null while getting MapInfo.  This is not expected.");
                    return result;
                }

                surface.Canvas.ClipRect(new SKRect((float)(x - 1), (float)(y - 1), (float)(x + 1), (float)(y + 1)));
                surface.Canvas.Clear(SKColors.Transparent);

                using var pixMap = surface.PeekPixels();
                var color = pixMap.GetPixelColor(intX, intY);

                for (var index = 0; index < mapInfoLayers.Count; index++)
                {
                    var mapList = list[index] = [];
                    var infoLayer = mapInfoLayers[index];
                    if (infoLayer is ILayerFeatureInfo layerFeatureInfo)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                // creating new list to avoid multithreading problems
                                mapList = [];
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
                                list[index] = mapList;
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
                        VisibleFeatureIterator.IterateLayers(viewport, [infoLayer], 0,
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

            // The VisibleFeatureIterator is intended for drawing and puts the bottom features first. In the MapInfo request
            // we want the top feature first. So, we reverse it here.
            var mapInfos = list.SelectMany(f => f).Reverse();
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
        _renderService.Dispose();
    }
}
