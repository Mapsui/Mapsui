using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Rendering.Skia;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using Mapsui.Widgets.InfoWidgets;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Styles;

[SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
public class CustomPointStyleAdvancedSample : ISample
{
    public string Name => $"{nameof(CustomPointStyle)} Advanced";
    public string Category => $"{nameof(CustomPointStyle)}";

    private static readonly Color _color1 = Color.FromString("#6A5ACD");
    private static readonly Color _color2 = Color.FromString("#7B68EE");
    private static readonly Color _color3 = Color.FromString("#9370DB");
    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(new MemoryLayer($"{nameof(CustomPointStyle)}")
        {
            Features = CreateFeatures(map.Extent!, 24).ToList(),
            Style = new StyleCollection
            {
                Styles =
                {
                    CreateCustomRendererStyle(),
                    new SymbolStyle() { SymbolScale = 0.2, Fill = new Brush(_color1) }, // Reference point at the center of the position
                }
            }
        });
        map.Widgets.Add(new MapInfoWidget(map, [map.Layers.Last()]));

        MapRenderer.RegisterPointStyleRenderer("custom-style-advanced", MyCustomStyleRenderer);

        return map;
    }

    private static void MyCustomStyleRenderer(SKCanvas canvas, IPointStyle style, RenderService renderService, float opacity)
    {
        var width = 30f;
        var halfWidth = width * 0.5f;
        var height = 30f;
        var halfHeight = height * 0.5f;

        // SymbolRotation, SymbolScale, RotateWithMap and Offset is already taken into account before this method is called.
        // RelativeOffset needs to be set in user code because in the caller the width and height is not known.
        var offset = style.RelativeOffset.GetAbsoluteOffset(width, height);
        canvas.Translate((float)offset.X, -(float)offset.Y);

        using var path = new SKPath();
        path.AddRect(new SKRect(-halfWidth, -halfHeight, -halfWidth * 0.5f, halfHeight));
        using var paint = new SKPaint { Color = _color1.ToSkia(), IsAntialias = true, Style = SKPaintStyle.StrokeAndFill, StrokeWidth = 0.5f };
        canvas.DrawPath(path, paint);

        using var path2 = new SKPath();
        path2.AddRect(new SKRect(-halfWidth * 0.5f, -halfHeight, halfWidth * 0.5f, halfHeight));
        using var paint2 = new SKPaint { Color = _color2.ToSkia(), IsAntialias = true, Style = SKPaintStyle.StrokeAndFill, StrokeWidth = 0.5f };
        canvas.DrawPath(path2, paint2);

        using var path3 = new SKPath();
        path3.AddRect(new SKRect(halfWidth * 0.5f, -halfHeight, halfWidth, halfHeight));
        using var paint3 = new SKPaint { Color = _color3.ToSkia(), IsAntialias = true, Style = SKPaintStyle.StrokeAndFill, StrokeWidth = 0.5f };
        canvas.DrawPath(path3, paint3);
    }

    private static ThemeStyle CreateCustomRendererStyle()
    {
        return new ThemeStyle((f) =>
        {
            // Get the counter that we assigned in CreateFeatures.
            var counter = (int)((PointFeature)f).Data!;

            // Here we use the counter to assign different settings to different features
            // to demonstrate that these are taken into account by the renderer.
            return new CustomPointStyle
            {
                RendererName = "custom-style-advanced",
                SymbolRotation = 90 * (counter % 4),
                RotateWithMap = counter % 2 == 0,
                SymbolScale = counter % 2 == 0 ? 0.8 : 1.2,
                Offset = new Offset(counter % 2 == 0 ? 0 : 10, counter % 2 == 0 ? 0 : 10),
                RelativeOffset = new RelativeOffset(counter % 2 == 0 ? -0.5 : 0.5, counter % 2 == 0 ? -0.5 : 0.5),
            };
        });
    }

    private static List<PointFeature> CreateFeatures(MRect envelope, int count)
    {
        var random = new Random(327);
        var randomPoints = RandomPointsBuilder.GenerateRandomPoints(envelope, count, random);
        var counter = 0;
        return randomPoints.Select(p => new PointFeature(p) { Data = counter++ }).ToList();
    }
}
