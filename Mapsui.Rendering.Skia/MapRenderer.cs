using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Manipulations;
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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Mapsui.Rendering.Skia;

public sealed class MapRenderer : IMapRenderer
{
    private long _currentIteration;
    private static readonly Dictionary<Type, IWidgetRenderer> _widgetRenderers = [];
    private static readonly Dictionary<Type, IStyleRenderer> _styleRenderers = [];
    private static readonly Dictionary<string, PointStyleRenderer.RenderHandler> _pointStyleRenderers = [];
    private static readonly Dictionary<string, CustomLayerRenderer.RenderHandler> _layerRenderers = [];

    static MapRenderer()
    {
        InitRenderer();

        DefaultRendererFactory.Create = () => new MapRenderer();
    }

    private static void InitRenderer()
    {
        _styleRenderers[typeof(RasterStyle)] = new RasterStyleRenderer();
        _styleRenderers[typeof(VectorStyle)] = new VectorStyleRenderer();
        _styleRenderers[typeof(LabelStyle)] = new LabelStyleRenderer();
        _styleRenderers[typeof(SymbolStyle)] = new SymbolStyleRenderer();
        _styleRenderers[typeof(ImageStyle)] = new ImageStyleRenderer();
        _styleRenderers[typeof(CustomPointStyle)] = new CustomPointStyleRenderer();
        _styleRenderers[typeof(CalloutStyle)] = new CalloutStyleRenderer();

        _widgetRenderers[typeof(TextBoxWidget)] = new TextBoxWidgetRenderer();
        _widgetRenderers[typeof(ScaleBarWidget)] = new ScaleBarWidgetRenderer();
        _widgetRenderers[typeof(ZoomInOutWidget)] = new ZoomInOutWidgetRenderer();
        _widgetRenderers[typeof(ImageButtonWidget)] = new ImageButtonWidgetRenderer();
        _widgetRenderers[typeof(BoxWidget)] = new BoxWidgetRenderer();
        _widgetRenderers[typeof(LoggingWidget)] = new LoggingWidgetRenderer();
        _widgetRenderers[typeof(InputOnlyWidget)] = new InputOnlyWidgetRenderer();
        _widgetRenderers[typeof(RulerWidget)] = new RulerWidgetRenderer();
        _widgetRenderers[typeof(PerformanceWidget)] = new PerformanceWidgetRenderer();
    }

    public void Render(object target, Viewport viewport, IEnumerable<ILayer> layers,
        IEnumerable<IWidget> widgets, RenderService renderService, Color? background = null)
    {
        var attributions = layers.Where(l => l.Enabled).Select(l => l.Attribution).Where(w => w != null).ToList();

        var allWidgets = widgets.Concat(attributions);

        RenderTypeSave((SKCanvas)target, viewport, layers, allWidgets, renderService, background);
    }

    private void RenderTypeSave(SKCanvas canvas, Viewport viewport, IEnumerable<ILayer> layers,
        IEnumerable<IWidget> widgets, RenderService renderService, Color? background = null)
    {
        if (!viewport.HasSize()) return;

        if (background is not null) canvas.Clear(background.ToSkia());
        Render(canvas, viewport, layers, renderService);
        Render(canvas, viewport, widgets, renderService, 1);
    }

    public MemoryStream RenderToBitmapStream(Map map, float pixelDensity = 1,
        RenderFormat renderFormat = RenderFormat.Png, int quality = 100)
    {
        return RenderToBitmapStream(map.Navigator.Viewport, map.Layers, map.RenderService, map.BackColor, pixelDensity, map.Widgets, renderFormat, quality);
    }

    public MemoryStream RenderToBitmapStream(Viewport viewport, IEnumerable<ILayer> layers, RenderService renderService,
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
                        RenderTo(viewport, layers, background, pixelDensity, widgets, renderService, skCanvas);
                        using var skPicture = pictureRecorder.EndRecording();
                        skPicture?.Serialize(memoryStream);
                        break;
                    }
                case RenderFormat.Png:
                    {
                        using var surface = SKSurface.Create(imageInfo);
                        using var skCanvas = surface.Canvas;
                        RenderTo(viewport, layers, background, pixelDensity, widgets, renderService, skCanvas);
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
                        RenderTo(viewport, layers, background, pixelDensity, widgets, renderService, skCanvas);
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
                        RenderTo(viewport, layers, background, pixelDensity, widgets, renderService, skCanvas);
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

    public bool TryGetWidgetRenderer(Type widgetType, [NotNullWhen(true)] out IWidgetRenderer? widgetRenderer)
    {
        if (_widgetRenderers.TryGetValue(widgetType, out var outWidgetRenderer))
        {
            widgetRenderer = outWidgetRenderer;
            return true;
        }
        widgetRenderer = null;
        return false;
    }

    public bool TryGetStyleRenderer(Type widgetType, [NotNullWhen(true)] out IStyleRenderer? styleRenderer)
    {
        if (_styleRenderers.TryGetValue(widgetType, out var outStyleRenderer))
        {
            styleRenderer = outStyleRenderer;
            return true;
        }
        styleRenderer = null;
        return false;
    }

    public static bool TryGetPointStyleRenderer(string rendererName, [NotNullWhen(true)] out PointStyleRenderer.RenderHandler? renderHandler)
    {
        if (_pointStyleRenderers.TryGetValue(rendererName, out var outRenderHandler))
        {
            renderHandler = outRenderHandler;
            return true;
        }
        renderHandler = null;
        return false;
    }

    public static void RegisterStyleRenderer(Type type, IStyleRenderer renderer)
    {
        _styleRenderers[type] = renderer;
    }

    public static void RegisterWidgetRenderer(Type type, IWidgetRenderer renderer)
    {
        _widgetRenderers[type] = renderer;
    }

    public static void RegisterPointStyleRenderer(string rendererName, PointStyleRenderer.RenderHandler rendererHandler)
    {
        _pointStyleRenderers[rendererName] = rendererHandler;
    }

    public static void RegisterLayerRenderer(string rendererName, CustomLayerRenderer.RenderHandler rendererHandler)
    {
        _layerRenderers[rendererName] = rendererHandler;
    }

    private void RenderTo(Viewport viewport, IEnumerable<ILayer> layers, Color? background, float pixelDensity,
        IEnumerable<IWidget>? widgets, RenderService renderService, SKCanvas skCanvas)
    {
        if (skCanvas == null) throw new ArgumentNullException(nameof(viewport));

        // Not sure if this is needed here:
        if (background is not null) skCanvas.Clear(background.ToSkia());
        skCanvas.Scale(pixelDensity, pixelDensity);
        Render(skCanvas, viewport, layers, renderService);
        if (widgets is not null)
            Render(skCanvas, viewport, widgets, renderService, 1);
    }

    private void Render(SKCanvas canvas, Viewport viewport, IEnumerable<ILayer> layers, RenderService renderService)
    {
        try
        {
            VisibleFeatureIterator.IterateLayers(viewport, layers, _currentIteration,
                (v, l, s, f, o, i) => RenderFeature(canvas, v, l, s, f, renderService, o, i),
                (l) => CustomLayerRendererCallback(canvas, viewport, l, renderService));

            _currentIteration++;
        }
        catch (Exception exception)
        {
            Logger.Log(LogLevel.Error, $"Unexpected error in skia renderer", exception);
        }
    }

    private static void CustomLayerRendererCallback(SKCanvas canvas, Viewport viewport, ILayer layer, RenderService renderService)
    {
        if (_layerRenderers.TryGetValue(layer.CustomLayerRendererName!, out var layerRenderer))
            CustomLayerRenderer.RenderLayer(canvas, viewport, layer, renderService, layerRenderer);
        else
            throw new Exception($"Layer renderer not found for {layer.GetType().Name}");
    }

    private static void RenderFeature(SKCanvas canvas, Viewport viewport, ILayer layer, IStyle style, IFeature feature,
        RenderService renderService, float layerOpacity, long iteration)
    {
        // Check, if we have a special renderer for this style
        if (_styleRenderers.TryGetValue(style.GetType(), out var renderer))
        {
            // Save canvas
            canvas.Save();
            // We have a special renderer, so try, if it could draw this
            var styleRenderer = (ISkiaStyleRenderer)renderer;
            styleRenderer.Draw(canvas, viewport, layer, feature, style, renderService, iteration);
            // Restore old canvas
            canvas.Restore();
        }
    }

    private void Render(object canvas, Viewport viewport, IEnumerable<IWidget> widgets, RenderService renderService, float layerOpacity)
    {
        WidgetRenderer.Render(canvas, viewport, widgets, _widgetRenderers, renderService, layerOpacity);
    }

    public MapInfo GetMapInfo(ScreenPosition screenPosition, Viewport viewport, IEnumerable<ILayer> layers, RenderService renderService, int margin = 0)
    {
        var mapInfoLayers = layers
            .Select(l => l is ISourceLayer sl and not ILayerFeatureInfo ? sl.SourceLayer : l)
            .ToList();

        var list = new ConcurrentQueue<List<MapInfoRecord>>();
        var mapInfo = new MapInfo(screenPosition, viewport.ScreenToWorld(screenPosition), viewport.Resolution);

        if (!viewport.ToExtent()?.Contains(viewport.ScreenToWorld(mapInfo.ScreenPosition)) ?? false) return mapInfo;

        try
        {
            var width = (int)viewport.Width;
            var height = (int)viewport.Height;

            var imageInfo = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Unpremul);

            var intX = (int)screenPosition.X;
            var intY = (int)screenPosition.Y;

            if (intX >= width || intY >= height)
                return mapInfo;

            using (var surface = SKSurface.Create(imageInfo))
            {
                if (surface == null)
                {
                    Logger.Log(LogLevel.Error, "SKSurface is null while getting MapInfo.  This is not expected.");
                    return mapInfo;
                }

                surface.Canvas.ClipRect(new SKRect((float)(screenPosition.X - 1), (float)(screenPosition.Y - 1), (float)(screenPosition.X + 1), (float)(screenPosition.Y + 1)));
                surface.Canvas.Clear(SKColors.Transparent);

                using var pixMap = surface.PeekPixels();
                var color = pixMap.GetPixelColor(intX, intY);

                for (var index = 0; index < mapInfoLayers.Count; index++)
                {
                    var mapList = new List<MapInfoRecord>();
                    list.Enqueue(mapList);
                    var infoLayer = mapInfoLayers[index];

                    // get information from ILayer
                    VisibleFeatureIterator.IterateLayers(viewport, [infoLayer], 0,
                        (v, layer, style, feature, opacity, iteration) =>
                        {
                            try
                            {
                                surface.Canvas.Save();
                                // 1) Clear the entire bitmap
                                surface.Canvas.Clear(SKColors.Transparent);
                                // 2) Render the feature to the clean canvas
                                RenderFeature(surface.Canvas, v, layer, style, feature, renderService, opacity, 0);
                                // 3) Check if the pixel has changed.
                                if (color != pixMap.GetPixelColor(intX, intY))
                                    // 4) Add feature and style to result
                                    mapList.Add(new MapInfoRecord(feature, style, layer));
                                surface.Canvas.Restore();
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

            // The VisibleFeatureIterator is intended for drawing and puts the bottom features first. In the MapInfo request
            // we want the top feature first. So, we reverse it here.
            var mapInfoRecords = list.SelectMany(f => f).Reverse().ToList();
            mapInfo = new MapInfo(screenPosition, viewport.ScreenToWorld(screenPosition), viewport.Resolution, mapInfoRecords);
        }
        catch (Exception exception)
        {
            Logger.Log(LogLevel.Error, $"Unexpected error in MapInfo skia renderer: {exception.Message}", exception);
        }

        return mapInfo;
    }
}
