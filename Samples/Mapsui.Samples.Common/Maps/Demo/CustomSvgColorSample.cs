using Mapsui.Layers;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using Mapsui.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Styles;
using System.Diagnostics.CodeAnalysis;
using Mapsui.Widgets.BoxWidgets;
using Mapsui.Extensions;

namespace Mapsui.Samples.Common.Maps.Demo;

[SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
public class CustomSvgStyleSample : ISample
{
    private const double _circumferenceOfTheEarth = 40075017;

    public string Name => "Custom Svg Color";
    public string Category => "Styles";
    private static string Description => "This samples applies custom colors for a specific element of an SVG." +
        "This allows users to change the fill and outline of an SVG with different colors.";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(new MemoryLayer("Custom Svg Style")
        {
            Features = RandomPointsBuilder.CreateRandomFeatures(map.Extent, 100).ToList(),
            Style = CreateDynamicSvgStyle()
        });
        map.Widgets.Add(CreateTextBox(Description));

        return Task.FromResult(map);
    }

    private static TextBoxWidget CreateTextBox(string description) => new()
    {
        Text = description,
        VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Top,
        HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Left,
        Margin = new MRect(10),
    };

    private static ThemeStyle CreateDynamicSvgStyle()
    {
        return new ThemeStyle((f) =>
        {
            var featurePosition = ((PointFeature)f).Point;
            var distance = Algorithms.Distance(new MPoint(0, 0), featurePosition);
            var distanceBetweenZeroAndOne = Math.Min(distance / _circumferenceOfTheEarth, 1);

            return new SymbolStyle
            {
                ImageSource = $"embedded://Mapsui.Samples.Common.Images.arrow.svg",
                SymbolOffset = new RelativeOffset(0.0, 0.5), // The point at the bottom should be at the location
                SvgFillColor = GetTypeColor((int)f.Id % 4),
                SvgStrokeColor = Color.Black,
                SymbolScale = 0.5,
                SymbolRotation = -CalculateAngle(new MPoint(0, 0), featurePosition) - 90, // Let them point to the center of hte map
                RotateWithMap = true,
            };
        });
    }

    private static Color GetTypeColor(int type) => type switch
    {
        0 => Color.FromString("#D8737F"),
        1 => Color.FromString("#AB6C82"),
        2 => Color.FromString("#685D79"),
        3 => Color.FromString("#475C7A"),
        _ => throw new Exception("Unknown type"),
    };

    private static double CalculateAngle(MPoint point1, MPoint point2)
    {
        var angleInRadians = Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);

        // Radians into degrees
        return angleInRadians * (180 / Math.PI);
    }
}
