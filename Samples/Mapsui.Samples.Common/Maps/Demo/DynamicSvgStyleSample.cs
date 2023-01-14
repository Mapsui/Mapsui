using Mapsui.Layers;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Samples.Common.Maps.Styles;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using Mapsui.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Demo;

public class DynamicSvgStyleSample : ISample
{
    public string Name => "Dynamic Svg Style";
    public string Category => "Styles";

    private string Description => "Tab or click in the map to see the change in symbols. This sample shows you can " +
        "change the size, rotation and color (using BlendModeColor) of a single SVG resource if you use a ThemeStyle";

    private const double circumferenceOfTheEarth = 40075017;

    public Task<Map> CreateMapAsync()
    {
        var infoPosition = new MPoint(); // Use closure to keep track of the info click position

        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(new MemoryLayer("Dynamic Svg Style")
        {
            Features = RandomPointsBuilder.CreateRandomFeatures(map.Extent, 1000),
            Style = CreateDynamicSvgStyle(() => infoPosition)
        });

        map.Info += (s, e) =>
        {
            if (e.MapInfo?.WorldPosition == null) return;
            infoPosition = e.MapInfo.WorldPosition; // Set the info position to use in the dynamic style
            map.Layers.First().DataHasChanged(); // To notify the map that a redraw is needed.
        };

        return Task.FromResult(map);
    }

    private IStyle CreateDynamicSvgStyle(Func<MPoint> getInfoPosition) // Use Func to make it get the latest clicked position
    {
        var bitmapId = typeof(SvgSample).LoadSvgId("Images.arrow.svg");

        return new ThemeStyle((f) =>
        {
            var featurePoint = ((PointFeature)f).Point;
            var distance = Algorithms.Distance(getInfoPosition(), featurePoint);
            var distanceBetweenZeroAndOne = Math.Min(distance / circumferenceOfTheEarth, 1);

            return new SymbolStyle
            {
                BitmapId = bitmapId,
                SymbolOffset = new RelativeOffset(0.0, 0.0),
                // 1. Change scale based on the distance
                SymbolScale = 0.25 + (0.25 * distanceBetweenZeroAndOne),
                // 2. Change angle pointing to the info click position
                SymbolRotation = -CalculateAngle(getInfoPosition(), featurePoint) - 90,
                RotateWithMap = true,
                Opacity = 0.9f,
                // 3. Use BlendModeColor to change the color of the svg
                BlendModeColor = ToColor(distanceBetweenZeroAndOne)
            };
        });
    }

    private static Color ToColor(double distanceBetweenZeroAndOne)
    {
        // Just improvising a bit with the color gradient.
        var red = 32;
        var green = 200 - (int)(distanceBetweenZeroAndOne * 200);
        var blue = 32;
        return Color.FromArgb(255, red, green, blue);
    }

    private double CalculateAngle(MPoint point1, MPoint point2)
    {
        // Use Atan2 for angle
        var radians = Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);

        // Radians into degrees
        return radians * (180 / Math.PI);
    }
}
