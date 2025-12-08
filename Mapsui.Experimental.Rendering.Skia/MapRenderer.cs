using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Manipulations;
using Mapsui.Experimental.Rendering.Skia.Extensions;
using Mapsui.Experimental.Rendering.Skia.SkiaStyles;
using Mapsui.Experimental.Rendering.Skia.SkiaWidgets;
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
using Mapsui.Rendering;
using Mapsui.Experimental.VectorTiles.Tiling;
using Mapsui.Experimental.Rendering.Skia.MapInfos;

namespace Mapsui.Experimental.Rendering.Skia;

/// <summary>
/// MapRenderer for SkiaSharp.
/// </summary>
public sealed class MapRenderer : IMapRenderer
{
    private long _currentIteration;
    private static readonly Dictionary<Type, ISkiaWidgetRenderer> _widgetRenderers = [];
    private static readonly Dictionary<Type, ISkiaStyleRenderer> _styleRenderers = [];
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
        _styleRenderers[typeof(VectorTileStyle)] = new VectorTileStyleRenderer();

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

    /// <summary>
    /// Renders the map to the target.
    /// </summary>
    /// <param name="target">The target to render to.</param>
    /// <param name="viewport">The viewport to render.</param>
    /// <param name="layers">The layers to render.</param>
    /// <param name="widgets">The widgets to render.</param>
    /// <param name="renderService">The render service.</param>
    /// <param name="background">The background color.</param>
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

    /// <summary>
    /// Renders the map to a bitmap stream.
    /// </summary>
    /// <param name="map">The map to render.</param>
    /// <param name="pixelDensity">The pixel density.</param>
    /// <param name="renderFormat">The render format.</param>
    /// <param name="quality">The quality of the image.</param>
    /// <returns>A memory stream containing the rendered image.</returns>
    public MemoryStream RenderToBitmapStream(Map map, float pixelDensity = 1,
        RenderFormat renderFormat = RenderFormat.Png, int quality = 100)
    {
        return RenderToBitmapStream(map.Navigator.Viewport, map.Layers, map.RenderService, map.BackColor, pixelDensity, map.Widgets, renderFormat, quality);
    }

    /// <summary>
    /// Renders the map to a bitmap stream.
    /// </summary>
    /// <param name="viewport">The viewport to render.</param>
    /// <param name="layers">The layers to render.</param>
    /// <param name="renderService">The render service.</param>
    /// <param name="background">The background color.</param>
    /// <param name="pixelDensity">The pixel density.</param>
    /// <param name="widgets">The widgets to render.</param>
    /// <param name="renderFormat">The render format.</param>
    /// <param name="quality">The quality of the image.</param>
    /// <returns>A memory stream containing the rendered image.</returns>
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

    /// <summary>
    /// Tries to get a widget renderer.
    /// </summary>
    /// <param name="widgetType">The type of the widget.</param>
    /// <param name="widgetRenderer">The widget renderer.</param>
    /// <returns>True if the renderer was found, false otherwise.</returns>
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

    /// <summary>
    /// Tries to get a style renderer.
    /// </summary>
    /// <param name="styleType">The type of the style.</param>
    /// <param name="styleRenderer">The style renderer.</param>
    /// <returns>True if the renderer was found, false otherwise.</returns>
    public bool TryGetStyleRenderer(Type styleType, [NotNullWhen(true)] out IStyleRenderer? styleRenderer)
    {
        if (_styleRenderers.TryGetValue(styleType, out var outStyleRenderer))
        {
            styleRenderer = outStyleRenderer;
            return true;
        }
        styleRenderer = null;
        return false;
    }

    /// <summary>
    /// Tries to get a point style renderer.
    /// </summary>
    /// <param name="rendererName">The name of the renderer.</param>
    /// <param name="renderHandler">The render handler.</param>
    /// <returns>True if the renderer was found, false otherwise.</returns>
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

    /// <summary>
    /// Registers a style renderer.
    /// </summary>
    /// <param name="type">The type of the style.</param>
    /// <param name="renderer">The renderer.</param>
    public static void RegisterStyleRenderer(Type type, ISkiaStyleRenderer renderer)
    {
        _styleRenderers[type] = renderer;
    }

    /// <summary>
    /// Registers a widget renderer.
    /// </summary>
    /// <param name="type">The type of the widget.</param>
    /// <param name="renderer">The renderer.</param>
    public static void RegisterWidgetRenderer(Type type, ISkiaWidgetRenderer renderer)
    {
        _widgetRenderers[type] = renderer;
    }

    /// <summary>
    /// Registers a point style renderer.
    /// </summary>
    /// <param name="rendererName">The name of the renderer.</param>
    /// <param name="rendererHandler">The renderer handler.</param>
    public static void RegisterPointStyleRenderer(string rendererName, PointStyleRenderer.RenderHandler rendererHandler)
    {
        _pointStyleRenderers[rendererName] = rendererHandler;
    }

    /// <summary>
    /// Registers a layer renderer.
    /// </summary>
    /// <param name="rendererName">The name of the renderer.</param>
    /// <param name="rendererHandler">The renderer handler.</param>
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
                (v, l, s, f, o, i) => RenderFeature(canvas, v, l, s, f, renderService, i),
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
        RenderService renderService, long iteration)
    {
        if (!_styleRenderers.TryGetValue(style.GetType(), out var styleRenderer))
            throw new Exception($"Style renderer not found for {style.GetType().Name}");

        var saveCount = canvas.Save(); // Save canvas
        styleRenderer.Draw(canvas, viewport, layer, feature, style, renderService, iteration);
        canvas.RestoreToCount(saveCount); // Restore old canvas
    }

    private static void Render(object canvas, Viewport viewport, IEnumerable<IWidget> widgets, RenderService renderService, float layerOpacity)
    {
        WidgetRenderer.Render(canvas, viewport, widgets, _widgetRenderers, renderService, layerOpacity);
    }

    /// <summary>
    /// Gets the map info.
    /// </summary>
    /// <param name="screenPosition">The screen position.</param>
    /// <param name="viewport">The viewport.</param>
    /// <param name="layers">The layers.</param>
    /// <param name="renderService">The render service.</param>
    /// <param name="margin">The margin.</param>
    /// <returns>The map info.</returns>
    public MapInfo GetMapInfo(ScreenPosition screenPosition, Viewport viewport, IEnumerable<ILayer> layers, RenderService renderService, int margin = 0)
    {
        var mapInfoLayers = layers
            .Select(l => l is ISourceLayer sl and not ILayerFeatureInfo ? sl.SourceLayer : l)
            .ToList();

        var list = new ConcurrentQueue<List<MapInfoRecord>>();
        var mapInfo = new MapInfo(screenPosition, viewport.ScreenToWorld(screenPosition), viewport.Resolution);

        if (!viewport.ToExtent()?.Contains(viewport.ScreenToWorld(mapInfo.ScreenPosition)) ?? false)
            return mapInfo;

        try
        {
            var width = (int)viewport.Width;
            var height = (int)viewport.Height;

            var imageInfo = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Unpremul);

            var intX = (int)screenPosition.X;
            var intY = (int)screenPosition.Y;

            if (intX >= width || intY >= height)
                return mapInfo;

            using var clearPixelPaint = new SKPaint { Color = SKColors.Transparent, BlendMode = SKBlendMode.Src };

            using var surface = SKSurface.Create(imageInfo);

            if (surface == null)
            {
                Logger.Log(LogLevel.Error, "SKSurface is null while getting MapInfo.  This is not expected.");
                return mapInfo;
            }

            surface.Canvas.ClipRect(new SKRect((float)(screenPosition.X - 1), (float)(screenPosition.Y - 1), (float)(screenPosition.X + 1), (float)(screenPosition.Y + 1)));
            surface.Canvas.Clear(SKColors.Transparent);

            using var pixMap = surface.PeekPixels();
            var originalColor = pixMap.GetPixelColor(intX, intY);

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
                            // 1) Clear the pixel where we clicked
                            surface.Canvas.DrawPoint(intX, intY, clearPixelPaint);

                            // 2) Render the feature to the clean canvas
                            if (!_styleRenderers.TryGetValue(style.GetType(), out var styleRenderer))
                                throw new Exception($"Style renderer not found for {style.GetType().Name}");

                            var saveCount = surface.Canvas.Save(); // Save canvas
                            styleRenderer.Draw(surface.Canvas, viewport, layer, feature, style, renderService, iteration);
                            surface.Canvas.RestoreToCount(saveCount); // Restore old canvas

                            // 3) Check if the pixel has changed.
                            if (originalColor != pixMap.GetPixelColor(intX, intY))
                            {
                                // 4) Add feature and style to result or dig deeper if it's a IMapInfoRenderer
                                if (styleRenderer is IMapInfoRenderer mapInfoRenderer)
                                {
                                    var subMapInfo = mapInfoRenderer.GetMapInfo(surface.Canvas, screenPosition, viewport, feature, style, layer, renderService);
                                    foreach (var record in subMapInfo)
                                    {
                                        mapList.Add(record);
                                    }
                                }
                                else
                                    mapList.Add(new MapInfoRecord(feature, style, layer));
                            }
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
