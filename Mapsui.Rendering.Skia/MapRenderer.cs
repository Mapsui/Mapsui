using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Rendering.Skia.SkiaWidgets;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidget;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.Zoom;
using SkiaSharp;

namespace Mapsui.Rendering.Skia;

public class MapRenderer : IRenderer
{
    private readonly IRenderCache _renderCache = new RenderCache();
    private long _currentIteration;

    public IRenderCache RenderCache => _renderCache;

    public IDictionary<Type, IWidgetRenderer> WidgetRenders { get; } = new Dictionary<Type, IWidgetRenderer>();

    /// <summary>
    /// Dictionary holding all special renderers for styles
    /// </summary>
    public IDictionary<Type, IStyleRenderer> StyleRenderers { get; } = new Dictionary<Type, IStyleRenderer>();

    static MapRenderer()
    {
        DefaultRendererFactory.Create = () => new MapRenderer();
    }

    public MapRenderer()
    {
        StyleRenderers[typeof(RasterStyle)] = new RasterStyleRenderer();
        StyleRenderers[typeof(VectorStyle)] = new VectorStyleRenderer();
        StyleRenderers[typeof(LabelStyle)] = new LabelStyleRenderer();
        StyleRenderers[typeof(SymbolStyle)] = new SymbolStyleRenderer();
        StyleRenderers[typeof(CalloutStyle)] = new CalloutStyleRenderer();

        WidgetRenders[typeof(Hyperlink)] = new HyperlinkWidgetRenderer();
        WidgetRenders[typeof(ScaleBarWidget)] = new ScaleBarWidgetRenderer();
        WidgetRenders[typeof(ZoomInOutWidget)] = new ZoomInOutWidgetRenderer();
        WidgetRenders[typeof(ButtonWidget)] = new ButtonWidgetRenderer();
    }

    public void Render(object target, IReadOnlyViewport viewport, IEnumerable<ILayer> layers,
        IEnumerable<IWidget> widgets, Color? background = null)
    {
        var attributions = layers.Where(l => l.Enabled).Select(l => l.Attribution).Where(w => w != null).ToList();

        var allWidgets = widgets.Concat(attributions);

        RenderTypeSave((SKCanvas)target, viewport, layers, allWidgets, background);
    }

    private void RenderTypeSave(SKCanvas canvas, IReadOnlyViewport viewport, IEnumerable<ILayer> layers,
        IEnumerable<IWidget> widgets, Color? background = null)
    {
        if (!viewport.HasSize()) return;

        if (background is not null) canvas.Clear(background.ToSkia());
        Render(canvas, viewport, layers);
        Render(canvas, viewport, widgets, 1);
    }

    public MemoryStream RenderToBitmapStream(IReadOnlyViewport viewport, IEnumerable<ILayer> layers,
        Color? background = null, float pixelDensity = 1, IEnumerable<IWidget>? widgets = null, RenderFormat renderFormat = RenderFormat.Png)
    {
        if (viewport == null) throw new ArgumentNullException(nameof(viewport));

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

    private void RenderTo(IReadOnlyViewport viewport, IEnumerable<ILayer> layers, Color? background, float pixelDensity,
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

    private void Render(SKCanvas canvas, IReadOnlyViewport viewport, IEnumerable<ILayer> layers)
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

    private void RenderFeature(SKCanvas canvas, IReadOnlyViewport viewport, ILayer layer, IStyle style, IFeature feature, float layerOpacity, long iteration)
    {
        // Check, if we have a special renderer for this style
        if (StyleRenderers.ContainsKey(style.GetType()))
        {
            // Save canvas
            canvas.Save();
            // We have a special renderer, so try, if it could draw this
            var styleRenderer = (ISkiaStyleRenderer)StyleRenderers[style.GetType()];
            var result = styleRenderer.Draw(canvas, viewport, layer, feature, style, _renderCache, iteration);
            // Restore old canvas
            canvas.Restore();
            // Was it drawn?
            if (result)
                // Yes, special style renderer drawn correct
                return;
        }
    }

    private void Render(object canvas, IReadOnlyViewport viewport, IEnumerable<IWidget> widgets, float layerOpacity)
    {
        WidgetRenderer.Render(canvas, viewport, widgets, WidgetRenders, layerOpacity);
    }

    public MapInfo? GetMapInfo(double x, double y, IReadOnlyViewport viewport, IEnumerable<ILayer> layers, int margin = 0)
    {
        // todo: use margin to increase the pixel area
        // todo: We will need to select on style instead of layer

        layers = layers
            .Select(l => (l is ISourceLayer sl) ? sl.SourceLayer : l)
            .Where(l => l.IsMapInfoLayer);

        var list = new List<MapInfoRecord>();
        var result = new MapInfo
        {
            ScreenPosition = new MPoint(x, y),
            WorldPosition = viewport.ScreenToWorld(x, y),
            Resolution = viewport.Resolution
        };

        if (!viewport.Extent?.Contains(viewport.ScreenToWorld(result.ScreenPosition)) ?? false) return result;

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

                using var pixmap = surface.PeekPixels();
                var color = pixmap.GetPixelColor(intX, intY);


                VisibleFeatureIterator.IterateLayers(viewport, layers, 0, (v, layer, style, feature, opacity, iteration) =>
                {
                    // ReSharper disable AccessToDisposedClosure // There is no delayed fetch. After IterateLayers returns all is done. I do not see a problem.
                    surface.Canvas.Save();
                    // 1) Clear the entire bitmap
                    surface.Canvas.Clear(SKColors.Transparent);
                    // 2) Render the feature to the clean canvas
                    RenderFeature(surface.Canvas, v, layer, style, feature, opacity, 0);
                    // 3) Check if the pixel has changed.
                    if (color != pixmap.GetPixelColor(intX, intY))
                        // 4) Add feature and style to result
                        list.Add(new MapInfoRecord(feature, style, layer));
                    surface.Canvas.Restore();
                    // ReSharper restore AccessToDisposedClosure
                });
            }

            if (list.Count == 0)
                return result;

            list.Reverse();
            var itemDrawnOnTop = list.First();

            result.Feature = itemDrawnOnTop.Feature;
            result.Style = itemDrawnOnTop.Style;
            result.Layer = itemDrawnOnTop.Layer;
            result.MapInfoRecords = list;

        }
        catch (Exception exception)
        {
            Logger.Log(LogLevel.Error, "Unexpected error in skia renderer", exception);
        }

        return result;
    }
}
