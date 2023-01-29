using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Styles;
using Mapsui.UI;
using System.IO;
using Mapsui.Extensions;
using Mapsui.Nts.Providers;
using Mapsui.Styles.Thematics;

#pragma warning disable IDISP001 // Dispose created

namespace Mapsui.Samples.Common.Maps.DataFormats;

public class GeoJsonSample : IMapControlSample
{
    static GeoJsonSample()
    {
        GeoJsonDeployer.CopyEmbeddedResourceToFile("cities.geojson");
    }

    public string Name => "13 GeoJson";
    public string Category => "Data Formats";

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();
    }

    public static Map CreateMap()
    {
        var map = new Map();
        var examplePath = Path.Combine(GeoJsonDeployer.GeoJsonLocation, "cities.geojson");
        var geoJson = new GeoJsonProvider(examplePath);
        map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateCityLayer(geoJson));
        map.Layers.Add(CreateCityLabelLayer(geoJson));

        return map;
    }

    private static ILayer CreateCityLayer(IProvider citySource)
    {
        return new Layer
        {
            Name = "Cities",
            DataSource = citySource,
            Style = CreateCityTheme()
        };
    }

    private static ILayer CreateCityLabelLayer(IProvider citiesProvider)
    {
        return new Layer("City labels")
        {
            DataSource = citiesProvider,
            Enabled = true,
            Style = CreateCityLabelStyle()
        };
    }

    private static IThemeStyle CreateCityTheme()
    {
        // Scaling city icons based on city population.
        // Cities below 1.000.000 gets the smallest symbol.
        // Cities with more than 5.000.000 the largest symbol.
        var bitmapId = typeof(ShapefileSample).LoadBitmapId(@"Images.icon.png");
        var cityMin = new SymbolStyle { BitmapId = bitmapId, SymbolScale = 0.5f };
        var cityMax = new SymbolStyle { BitmapId = bitmapId, SymbolScale = 1f };
        return new GradientTheme("Population", 1000000, 5000000, cityMin, cityMax);
    }

    private static LabelStyle CreateCityLabelStyle()
    {
        return new LabelStyle
        {
            ForeColor = Color.Black,
            BackColor = new Brush { Color = Color.Orange },
            Font = new Font { FontFamily = "GenericSerif", Size = 11 },
            HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
            VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center,
            Offset = new Offset { X = 0, Y = 0 },
            Halo = new Pen { Color = Color.Yellow, Width = 2 },
            CollisionDetection = true,
            LabelColumn = "NAME"
        };
    }
}
