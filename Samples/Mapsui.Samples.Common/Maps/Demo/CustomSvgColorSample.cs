using Mapsui.Layers;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using Mapsui.Utilities;
using Svg.Model;
using Svg.Skia;
using Svg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SkiaSharp;
using Mapsui.Styles;
using System.Diagnostics.CodeAnalysis;
using Mapsui.Samples.Common.Maps.Styles;

namespace Mapsui.Samples.Common.Maps.Demo;

[SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
public class CustomSvgStyleSample : ISample
{
    public string Name => "Custom Svg Color";
    public string Category => "Styles";
    private string Description => "This samples applies custom colors for a specific element of an SVG. This would, for instance, allow users to change the fill and outline of an SVG with different colors.";

    private const double circumferenceOfTheEarth = 40075017;
    private Random _random = new Random(1337);

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(new MemoryLayer("Custom Svg Style")
        {
            Features = RandomPointsBuilder.CreateRandomFeatures(map.Extent, 100).Select(f =>
                {
                    // Add four random types to use in the style
                    f["type"] = _random.Next(4);
                    return f;
                }).ToList(),

            Style = CreateDynamicSvgStyle()
        });

        return Task.FromResult(map);
    }

    private IStyle CreateDynamicSvgStyle() // Use Func to make it get the latest clicked position
    {
        var bitmapIds = new[] {
            LoadBitmap(0),
            LoadBitmap(1),
            LoadBitmap(2),
            LoadBitmap(3),
        };

        return new ThemeStyle((f) =>
        {
            var featurePosition = ((PointFeature)f).Point;
            var distance = Algorithms.Distance(new MPoint(0, 0), featurePosition);
            var distanceBetweenZeroAndOne = Math.Min(distance / circumferenceOfTheEarth, 1);
            var bitmapId = bitmapIds[(int)f["type"]!];

            return new SymbolStyle
            {
                BitmapId = bitmapId,
                SymbolOffset = new RelativeOffset(0.0, 0.0),
                SymbolScale = 0.5,
                // Let them point to the center of hte map
                SymbolRotation = -CalculateAngle(new MPoint(0, 0), featurePosition) - 90,
                RotateWithMap = true,
            };
        });
    }

    private static int LoadBitmap(int type)
    {
        var bitmapPath = "Images.arrow.svg";
        using var bitmapData = EmbeddedResourceLoader.Load(bitmapPath, typeof(SvgSample));
        var skPicture = SvgLoader.ToSKPicture(bitmapData, ToSystemDrawingColor(GetTypeColor(type))) 
            ?? throw new Exception($"Failed to load bitmap: {bitmapPath}");
        return BitmapRegistry.Instance.Register(skPicture);
    }

    private static Color GetTypeColor(int type) => type switch
    {
        0 => Color.FromString("#D8737F"),
        1 => Color.FromString("#AB6C82"),
        2 => Color.FromString("#685D79"),
        3 => Color.FromString("#475C7A"),
        _ => throw new Exception("Unknown type"),
    };

    public static System.Drawing.Color ToSystemDrawingColor(Color color) => 
        System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);

    private double CalculateAngle(MPoint point1, MPoint point2)
    {
        // Use Atan2 for angle
        var radians = Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);

        // Radians into degrees
        return radians * (180 / Math.PI);
    }

    public class SvgLoader
    {
        public static SKPicture? ToSKPicture(Stream stream, System.Drawing.Color? fillColor = null, System.Drawing.Color? strokeColor = null)
        {
            var svgDocument = SvgExtensions.Open(stream);
            if (svgDocument is null) return null;

            var elements = GetAllElements(svgDocument.Children);
            foreach (var element in elements)
            {
                if (element.Fill is not null && fillColor is { })
                    element.Fill = new SvgColourServer(fillColor.Value);
                if (element.Stroke is not null && strokeColor is { })
                    element.Stroke = new SvgColourServer(strokeColor.Value);
            }

            return ToSKPicture(svgDocument);
        }

        private static SKPicture? ToSKPicture(SvgDocument svgDocument)
        {
            var skiaModel = new SkiaModel(new SKSvgSettings());
            var assetLoader = new SkiaAssetLoader(skiaModel);
            var model = SvgExtensions.ToModel(svgDocument, assetLoader, out var _, out _);
            return skiaModel.ToSKPicture(model);
        }

        public static List<SvgElement> GetAllElements(SvgElementCollection elements)
        {
            var result = new List<SvgElement>();
            foreach (var element in elements)
            {
                result.Add(element);

                if (element.Children.Count > 0)
                    result.AddRange(GetAllElements(element.Children));
            }
            return result;
        }
    }
}
