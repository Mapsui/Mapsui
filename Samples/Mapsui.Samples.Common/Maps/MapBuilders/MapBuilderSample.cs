using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.MapBuilders;

public class MapBuilderSample : ISample
{
    readonly MPoint _sphericalMercatorCoordinate = SphericalMercator.FromLonLat(-81.2497, 42.9837).ToMPoint();

    public string Name => "MapBuilder";
    public string Category => "MapBuilders";

    public Task<Map> CreateMapAsync()
        => Task.FromResult(new MapBuilder()
            .WithOpenStreetMapLayer((l, m) => l.Name = "OpenStreetMap")
            .WithLayer((map) => new MemoryLayer("Pin Layer") { Features = CreateFeatures(), IsMapInfoLayer = true },
                (l, map) => l.WithPinWithCalloutLayer(map))
            .WithZoomButtons()
            .WithScaleBarWidget(w =>
            {
                w.Margin = new MRect(16);
                w.Halo = Color.WhiteSmoke; // It is possible to set properties of derived classes.
            })
            .WithMapCRS("EPSG:3857") // Should we have such specific methods or should the configure method be enough?
            .WithMapConfiguration(map => map.CRS = "EPSG:3857") // Does the same thing as the line above.
            .WithMapConfiguration(map => map.Navigator.CenterOnAndZoomTo(_sphericalMercatorCoordinate, 1222.99)) // Navigation is complex, because the Map is passed as argument the navigation methods could be called. Better to have specific builder methods for navigation.
            .Build());

    private IEnumerable<IFeature> CreateFeatures()
        => [new PointFeature(_sphericalMercatorCoordinate) { Data = new UserData { CalloutText = "Hello!" } }];
}

public static class SampleMapBuilderExtensions
{
    public static ILayer WithPinWithCalloutLayer(this ILayer layer, Map map)
    {
        map.Info += (sender, args) =>
        {
            if (args.MapInfo.Feature?.Data is UserData data)
                data.CalloutEnabled = !data.CalloutEnabled;
        };

        layer.WithPinAndCallout(
            (f) => ((UserData)f.Data!).CalloutEnabled,
            (f) => ((UserData)f.Data!).CalloutText);

        return layer;
    }
}

public static class SampleLayerExtensions
{
    public static ILayer WithPinAndCallout(this ILayer layer, Func<IFeature, bool> enabledFromFeature, Func<IFeature, string> calloutTextFromFeature)
    {
        layer.Style = new StyleCollection
        {
            Styles =
            {
                new SymbolStyle
                {
                    ImageSource = "embedded://Mapsui.Resources.Images.Pin.svg",
                    SymbolOffset = new RelativeOffset(0.0, 0.5), // The point at the bottom should be at the location
                    SvgFillColor = Color.CornflowerBlue,
                    SvgStrokeColor = Color.Black,
                    SymbolScale = 1,
                },
                new ThemeStyle(f =>
                {
                    return new CalloutStyle()
                    {
                        Enabled = enabledFromFeature(f),
                        SymbolOffset = new Offset(0, 52),
                        TitleFont = { FontFamily = null, Size = 24, Italic = false, Bold = true },
                        TitleFontColor = Color.Black,
                        Type = CalloutType.Single,
                        MaxWidth = 120,
                        Title = calloutTextFromFeature(f),
                    };
                })
            }
        };

        return layer;
    }
}

public class UserData
{
    public bool CalloutEnabled { get; set; }
    public string CalloutText { get; set; } = string.Empty;
}
